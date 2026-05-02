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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UsuarioRol>()
            .HasOne(ur => ur.Usuario)
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UsuarioId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Alumno>()
            .HasOne(a => a.Usuario)
            .WithOne(u => u.Alumno)
            .HasForeignKey<Alumno>(a => a.UsuarioId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Coordinador>()
            .HasOne(c => c.Usuario)
            .WithOne(u => u.Coordinador)
            .HasForeignKey<Coordinador>(c => c.UsuarioId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JefeDepartamento>()
            .HasOne(jd => jd.Usuario)
            .WithOne(u => u.JefeDepartamento)
            .HasForeignKey<JefeDepartamento>(jd => jd.UsuarioId)
            .IsRequired()
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
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AsesorInterno>()
            .HasOne(ai => ai.Usuario)
            .WithOne(u => u.AsesorInterno)
            .HasForeignKey<AsesorInterno>(ai => ai.UsuarioId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AsesorExterno>()
            .HasOne(ae => ae.Usuario)
            .WithOne(u => u.AsesorExterno)
            .HasForeignKey<AsesorExterno>(ae => ae.UsuarioId)
            .IsRequired()
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

        modelBuilder.Entity<DocumentoAlumno>()
            .HasOne(da => da.Alumno)
            .WithMany(a => a.Documentos)
            .HasForeignKey(da => da.AlumnoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentoAcuerdo>(entity =>
        {
            entity.HasOne(d => d.Alumno)
                .WithMany()
                .HasForeignKey(d => d.AlumnoId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Profesor)
                .WithMany()
                .HasForeignKey(d => d.ProfesorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.AsesorInterno)
                .WithMany()
                .HasForeignKey(d => d.AsesorInternoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.AsesorExterno)
                .WithMany()
                .HasForeignKey(d => d.AsesorExternoId)
                .OnDelete(DeleteBehavior.Restrict);
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
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var propertyMethodInfo = typeof(ISoftDelete).GetProperty(nameof(ISoftDelete.IsDeleted));
                var isDeletedProperty = Expression.Property(parameter, propertyMethodInfo!);
                var compareExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                var lambda = Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }

            if (typeof(IActivatable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var propertyMethodInfo = typeof(IActivatable).GetProperty(nameof(IActivatable.IsActive));
                var isActiveProperty = Expression.Property(parameter, propertyMethodInfo!);
                var compareExpression = Expression.Equal(isActiveProperty, Expression.Constant(true));
                var lambda = Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
