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
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<Coordinador> Coordinadores { get; set; } = null!;
    public DbSet<JefeDepartamento> JefesDepartamento { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasMany(u => u.Roles)
            .WithOne(ur => ur.Usuario)
            .HasForeignKey(ur => ur.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Alumno)
            .WithOne(a => a.Usuario)
            .HasForeignKey<Alumno>(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Coordinador)
            .WithOne(c => c.Usuario)
            .HasForeignKey<Coordinador>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.JefeDepartamento)
            .WithOne(jd => jd.Usuario)
            .HasForeignKey<JefeDepartamento>(jd => jd.UsuarioId)
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

        modelBuilder.Entity<Alumno>()
            .HasMany(a => a.Documentos)
            .WithOne(da => da.Alumno)
            .HasForeignKey(da => da.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentoAcuerdo>(entity =>
        {
            entity.HasOne(d => d.Alumno)
                .WithMany()
                .HasForeignKey(d => d.AlumnoId)
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
        }
    }
}
