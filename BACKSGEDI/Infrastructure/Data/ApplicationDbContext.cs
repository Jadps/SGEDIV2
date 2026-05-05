using BACKSGEDI.Domain.Entities;
using BACKSGEDI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BACKSGEDI.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<UsuarioRol> UsuariosRoles { get; set; } = null!;
    public DbSet<Alumno> Alumnos { get; set; } = null!;
    public DbSet<CatCarrera> Carreras { get; set; } = null!;
    public DbSet<DocumentoAlumno> DocumentosAlumnos { get; set; } = null!;
    public DbSet<DocumentoAcuerdo> DocumentosAcuerdos { get; set; } = null!;
    public DbSet<PlantillaDocumento> PlantillasDocumentos { get; set; } = null!;
    public DbSet<ConfiguracionFechaLimite> ConfiguracionesFechasLimites { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<Coordinador> Coordinadores { get; set; } = null!;
    public DbSet<JefeDepartamento> JefesDepartamento { get; set; } = null!;
    public DbSet<Profesor> Profesores { get; set; } = null!;
    public DbSet<AsesorInterno> AsesoresInternos { get; set; } = null!;
    public DbSet<AsesorExterno> AsesoresExternos { get; set; } = null!;
    public DbSet<Empresa> Empresas { get; set; } = null!;
    public DbSet<Materia> Materias { get; set; } = null!;
    public DbSet<CargaAcademica> CargasAcademicas { get; set; } = null!;
    public DbSet<EvaluacionDesempeño> EvaluacionesDesempeño { get; set; } = null!;
    public DbSet<ContratoProfesor> ContratosProfesores { get; set; } = null!;
    public DbSet<CriterioEvaluacion> CriteriosEvaluacion { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UsuarioRol>()
            .HasOne(ur => ur.Usuario)
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UsuarioId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Alumno>(entity =>
        {
            entity.HasOne(a => a.Usuario)
                .WithOne(u => u.Alumno)
                .HasForeignKey<Alumno>(a => a.UsuarioId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Empresa)
                .WithMany()
                .HasForeignKey(a => a.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.AsesorInterno)
                .WithMany()
                .HasForeignKey(a => a.AsesorInternoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.AsesorExterno)
                .WithMany()
                .HasForeignKey(a => a.AsesorExternoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Coordinador>()
            .HasOne(c => c.Usuario)
            .WithOne(u => u.Coordinador)
            .HasForeignKey<Coordinador>(c => c.UsuarioId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JefeDepartamento>()
            .HasOne(jd => jd.Usuario)
            .WithOne(u => u.JefeDepartamento)
            .HasForeignKey<JefeDepartamento>(jd => jd.UsuarioId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Coordinador>()
            .HasOne(c => c.Carrera)
            .WithMany()
            .HasForeignKey(c => c.CarreraId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<JefeDepartamento>()
            .HasOne(jd => jd.Carrera)
            .WithMany()
            .HasForeignKey(jd => jd.CarreraId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Profesor>()
            .HasOne(p => p.Usuario)
            .WithOne(u => u.Profesor)
            .HasForeignKey<Profesor>(p => p.UsuarioId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AsesorInterno>()
            .HasOne(ai => ai.Usuario)
            .WithOne(u => u.AsesorInterno)
            .HasForeignKey<AsesorInterno>(ai => ai.UsuarioId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AsesorExterno>()
            .HasOne(ae => ae.Usuario)
            .WithOne(u => u.AsesorExterno)
            .HasForeignKey<AsesorExterno>(ae => ae.UsuarioId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AsesorExterno>()
            .HasOne(ae => ae.Empresa)
            .WithMany(e => e.AsesoresExternos)
            .HasForeignKey(ae => ae.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Materia>()
            .HasOne(m => m.Carrera)
            .WithMany()
            .HasForeignKey(m => m.CarreraId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CargaAcademica>(entity =>
        {
            entity.HasOne(ca => ca.Alumno)
                .WithMany()
                .HasForeignKey(ca => ca.AlumnoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ca => ca.Materia)
                .WithMany()
                .HasForeignKey(ca => ca.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ca => ca.Profesor)
                .WithMany()
                .HasForeignKey(ca => ca.ProfesorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentoAlumno>()
            .HasOne(da => da.Alumno)
            .WithMany(a => a.Documentos)
            .HasForeignKey(da => da.AlumnoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentoAcuerdo>(entity =>
        {
            entity.HasOne(d => d.Alumno)
                .WithMany()
                .HasForeignKey(d => d.AlumnoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Profesor)
                .WithMany()
                .HasForeignKey(d => d.ProfesorId)
                .OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<EvaluacionDesempeño>(entity =>
        {
            entity.HasOne(e => e.Alumno)
                .WithMany()
                .HasForeignKey(e => e.AlumnoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Evaluador)
                .WithMany()
                .HasForeignKey(e => e.EvaluadorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContratoProfesor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Alumno).WithMany().HasForeignKey(e => e.AlumnoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Materia).WithMany().HasForeignKey(e => e.MateriaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Criterios).WithOne().OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CriterioEvaluacion>(entity =>
        {
            entity.HasOne(c => c.Contrato)
                .WithMany(a => a.Criterios)
                .HasForeignKey(c => c.ContratoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Alumno>().HasIndex(a => a.Matricula).IsUnique();

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
            entity.Property(e => e.ChangedColumns).HasColumnType("jsonb");
            entity.Property(e => e.PrimaryKey).HasColumnType("jsonb");
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasStatus).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var propertyMethodInfo = typeof(IHasStatus).GetProperty(nameof(IHasStatus.Status));
                var statusProperty = Expression.Property(parameter, propertyMethodInfo!);
                var compareExpression = Expression.NotEqual(statusProperty, Expression.Constant(3));
                var lambda = Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
