using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BACKSGEDI.Configuration;
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
    public IFormFile AnexoIFile { get; set; } = null!;
    public IFormFile KardexFile { get; set; } = null!;
}

public class RegisterStudentValidator : Validator<RegisterStudentRequest>
{
    public RegisterStudentValidator(IOptions<StorageOptions> storageOptions)
    {
        var settings = storageOptions.Value;

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

        RuleFor(x => x.HorarioFile).NotNull().Must(f => IsValidPdfAndSize(f, settings.MaxFileSizeInBytes))
            .WithMessage($"El horario debe ser PDF y menor a {settings.MaxFileSizeInBytes/1024/1024}MB.");
        RuleFor(x => x.AnexoIFile).NotNull().Must(f => IsValidPdfAndSize(f, settings.MaxFileSizeInBytes))
            .WithMessage($"El Anexo I debe ser PDF y menor a {settings.MaxFileSizeInBytes/1024/1024}MB.");
        RuleFor(x => x.KardexFile).NotNull().Must(f => IsValidPdfAndSize(f, settings.MaxFileSizeInBytes))
            .WithMessage($"El Kardex debe ser PDF y menor a {settings.MaxFileSizeInBytes/1024/1024}MB.");
    }
    private static bool IsValidPdfAndSize(IFormFile? file, long maxSize)
    {
        if (file == null) return false; 

        var isPdf = file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                    file.ContentType == "application/pdf";

        return isPdf && file.Length <= maxSize;
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

        var semestreActual = SemestreHelper.GetSemestreActual();

        try
        {
            var pathHorario = await _storageService.UploadFileAsync(req.HorarioFile, alumnoId.ToString(), TipoDocumentoAlumno.Horario.ToString(), semestreActual, ct);
            var pathAnexoI = await _storageService.UploadFileAsync(req.AnexoIFile, alumnoId.ToString(), TipoAcuerdo.AnexoI.ToString(), semestreActual, ct);
            var pathKardex = await _storageService.UploadFileAsync(req.KardexFile, alumnoId.ToString(), TipoDocumentoAlumno.Kardex.ToString(), semestreActual, ct);
            alumno.Documentos.Add(new DocumentoAlumno 
            { 
                AlumnoId = alumnoId, 
                TipoDocumento = TipoDocumentoAlumno.Horario, 
                RutaArchivo = pathHorario, 
                Semestre = semestreActual,
                SubidoPorUsuarioId = userId,
                Version = 1,
                EsVersionActual = true
            });
            
            alumno.Documentos.Add(new DocumentoAlumno 
            { 
                AlumnoId = alumnoId, 
                TipoDocumento = TipoDocumentoAlumno.Kardex, 
                RutaArchivo = pathKardex, 
                Semestre = semestreActual,
                SubidoPorUsuarioId = userId,
                Version = 1,
                EsVersionActual = true
            });

            var docAcuerdoAnexoI = new DocumentoAcuerdo
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumnoId,
                TipoAcuerdo = TipoAcuerdo.AnexoI,
                Semestre = semestreActual,
                RutaArchivo = pathAnexoI,
                FechaSubida = DateTime.UtcNow,
                FechaLimite = FechasLimiteService.GetDefaultFechaLimite(semestreActual),
                SubidoPorUsuarioId = userId,
                Estado = EstadoDocumento.PendienteRevision,
                Version = 1,
                EsVersionActual = true
            };

            await _db.Usuarios.AddAsync(user, ct);
            await _db.Alumnos.AddAsync(alumno, ct);
            await _db.DocumentosAcuerdos.AddAsync(docAcuerdoAnexoI, ct);
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