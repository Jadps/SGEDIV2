using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Services;
using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Enums;
using BACKSGEDI.Domain.Common;
using BACKSGEDI.Infrastructure.Extensions;

namespace BACKSGEDI.Features.Auth.RegisterStudent;

public class RegisterStudentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public int CarreraId { get; set; }
    public int SemestreId { get; set; }

    public IFormFile HorarioFile { get; set; } = null!;
    public IFormFile Anexo1File { get; set; } = null!;
    public IFormFile KardexFile { get; set; } = null!;
}

public class RegisterStudentValidator : Validator<RegisterStudentRequest>
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    public RegisterStudentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .Matches(@"^L\d{2}25\d{4}@tlalnepantla\.tecnm\.mx$")
            .WithMessage("El correo debe ser institucional del TecNM.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("La contraseña requiere al menos 6 caracteres.");
        RuleFor(x => x.Matricula).NotEmpty().WithMessage("La matrícula es requerida.");
        RuleFor(x => x.CarreraId).GreaterThan(0).WithMessage("Selecciona una carrera válida.");
        RuleFor(x => x.SemestreId)
            .GreaterThanOrEqualTo(6)
            .WithMessage("El alumno debe cursar al menos el sexto semestre para el modelo dual.");

        RuleFor(x => x.HorarioFile).NotNull().Must(IsValidPdfAndSize!).When(x => x.HorarioFile != null);
        RuleFor(x => x.Anexo1File).NotNull().Must(IsValidPdfAndSize!).When(x => x.Anexo1File != null);
        RuleFor(x => x.KardexFile).NotNull().Must(IsValidPdfAndSize!).When(x => x.KardexFile != null);
    }

    private bool IsValidPdfAndSize(IFormFile file)
    {
        var isPdf = file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                    file.ContentType == "application/pdf";
        return isPdf && file.Length <= MaxFileSize;
    }
}

public class RegisterStudentEndpoint : FastEndpoints.Endpoint<RegisterStudentRequest>
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storageService;

    public RegisterStudentEndpoint(ApplicationDbContext db, IStorageService storageService)
    {
        _db = db;
        _storageService = storageService;
    }

    public override void Configure()
    {
        Post("/api/auth/register-student");
        AllowAnonymous();
        AllowFileUploads();
    }

    public override async Task HandleAsync(RegisterStudentRequest req, CancellationToken ct)
    {
        var result = await RegisterAsync(req, ct);
        await result.ToResult().ExecuteAsync(HttpContext);
    }

    private async Task<Result> RegisterAsync(RegisterStudentRequest req, CancellationToken ct)
    {
        var emailExists     = await _db.Usuarios.AnyAsync(u => u.Email == req.Email, ct);
        var matriculaExists = await _db.Alumnos.AnyAsync(a => a.Matricula == req.Matricula, ct);

        if (emailExists || matriculaExists)
        {
            return Result.Failure(Error.Conflict("Student.AlreadyExists",
                "El email o matrícula ya se encuentran registrados."));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var userId   = Guid.NewGuid();
        var alumnoId = Guid.NewGuid();

        var user = new Usuario
        {
            Id           = userId,
            Name         = req.Name,
            Email        = req.Email,
            PasswordHash = passwordHash,
            Roles        = new List<UsuarioRol>
            {
                new UsuarioRol { Role = "Alumno" }
            },
            IsActive = false
        };

        var alumno = new Alumno
        {
            Id         = alumnoId,
            UsuarioId  = userId,
            Matricula  = req.Matricula,
            CarreraId  = req.CarreraId,
            SemestreId = req.SemestreId
        };

        var userIdStr = userId.ToString();

        var semestreActual = SemestreHelper.GetSemestreActual();

        try
        {
            var pathHorario = await _storageService.UploadFileAsync(req.HorarioFile, alumnoId.ToString(), "Horarios", semestreActual, ct);
            var pathAnexo   = await _storageService.UploadFileAsync(req.Anexo1File,  alumnoId.ToString(), "Anexos",   semestreActual, ct);
            var pathKardex  = await _storageService.UploadFileAsync(req.KardexFile,  alumnoId.ToString(), "Kardex",   semestreActual, ct);

            alumno.Documentos.Add(new DocumentoAlumno 
            { 
                AlumnoId = alumnoId, 
                TipoDocumento = TipoDocumentoAlumno.Horario, 
                RutaArchivo = pathHorario, 
                Semestre = semestreActual,
                SubidoPorUsuarioId = userId
            });
            
            alumno.Documentos.Add(new DocumentoAlumno 
            { 
                AlumnoId = alumnoId, 
                TipoDocumento = TipoDocumentoAlumno.Kardex, 
                RutaArchivo = pathKardex, 
                Semestre = semestreActual,
                SubidoPorUsuarioId = userId
            });

            var docAcuerdoAnexo1 = new DocumentoAcuerdo
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumnoId,
                TipoAcuerdo = TipoAcuerdo.AnexoI,
                Semestre = semestreActual,
                RutaArchivo = pathAnexo,
                FechaSubida = DateTime.UtcNow,
                FechaLimite = FechasLimiteService.GetFechaLimite(TipoAcuerdo.AnexoI, semestreActual),
                SubidoPorUsuarioId = userId,
                Estado = EstadoDocumento.PendienteRevision
            };

            await _db.Usuarios.AddAsync(user, ct);
            await _db.Alumnos.AddAsync(alumno, ct);
            await _db.DocumentosAcuerdos.AddAsync(docAcuerdoAnexo1, ct);
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _storageService.DeleteStudentFolder(alumnoId.ToString());
            return Result.Failure(Error.Failure("Registration.Failed",
                $"Error al registrar el alumno: {ex.Message}"));
        }

        return Result.Success();
    }
}
