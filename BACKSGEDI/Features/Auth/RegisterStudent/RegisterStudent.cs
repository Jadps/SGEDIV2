using FastEndpoints;
using FluentValidation;
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
    public IFormFile Anexo1File { get; set; } = null!;
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
        
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Mínimo 6 caracteres.");
        RuleFor(x => x.Matricula).NotEmpty().WithMessage("La matrícula es requerida.");
        
        // Validación de Archivos usando la configuración global
        RuleFor(x => x.HorarioFile).NotNull().Must(f => IsValidPdfAndSize(f, settings.MaxFileSizeInBytes))
            .WithMessage($"El horario debe ser PDF y menor a {settings.MaxFileSizeInBytes / 1024 / 1024}MB.");
        
        RuleFor(x => x.Anexo1File).NotNull().Must(f => IsValidPdfAndSize(f, settings.MaxFileSizeInBytes))
            .WithMessage("El Anexo 1 debe ser PDF y respetar el límite de tamaño.");
            
        RuleFor(x => x.KardexFile).NotNull().Must(f => IsValidPdfAndSize(f, settings.MaxFileSizeInBytes))
            .WithMessage("El Kardex debe ser PDF y respetar el límite de tamaño.");
    }

    private static bool IsValidPdfAndSize(IFormFile file, long maxSize)
    {
        var isPdf = file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                    file.ContentType == "application/pdf";
        return isPdf && file.Length <= maxSize;
    }
}

public class RegisterStudentEndpoint : Endpoint<RegisterStudentRequest>
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
        // ... (Checks de existencia iguales)

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        var userId = Guid.NewGuid();
        var alumnoId = Guid.NewGuid();
        var semestreActual = SemestreHelper.GetSemestreActual();

        // 1. Crear Entidades Base
        var user = new Usuario { /* ... tus campos ... */ IsActive = false };
        var alumno = new Alumno { Id = alumnoId, UsuarioId = userId, /* ... */ };

        try
        {
            // 2. Subida de archivos
            var pathHorario = await _storageService.UploadFileAsync(req.HorarioFile, alumnoId.ToString(), "Horarios", semestreActual, ct);
            var pathAnexo = await _storageService.UploadFileAsync(req.Anexo1File, alumnoId.ToString(), "Anexos", semestreActual, ct);
            var pathKardex = await _storageService.UploadFileAsync(req.KardexFile, alumnoId.ToString(), "Kardex", semestreActual, ct);

            // 3. Documentos con Versionado (Crucial para SGEDI v2)
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
                Estado = EstadoDocumento.PendienteRevision,
                Version = 1,
                EsVersionActual = true
            };

            await _db.Usuarios.AddAsync(user, ct);
            await _db.Alumnos.AddAsync(alumno, ct);
            await _db.DocumentosAcuerdos.AddAsync(docAcuerdoAnexo1, ct);
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _storageService.DeleteStudentFolder(alumnoId.ToString());
            return Result.Failure(Error.Failure("Registration.Failed", $"Error: {ex.Message}"));
        }

        return Result.Success();
    }
}