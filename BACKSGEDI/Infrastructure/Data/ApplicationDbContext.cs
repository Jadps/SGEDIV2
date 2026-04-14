using BACKSGEDI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Alumno> Alumnos { get; set; } = null!;
    public DbSet<CatCarrera> Carreras { get; set; } = null!;
    public DbSet<DocumentoAlumno> DocumentosAlumnos { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<Coordinador> Coordinadores { get; set; } = null!;
    public DbSet<JefeDepartamento> JefesDepartamento { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Alumno>().HasIndex(a => a.Matricula).IsUnique();

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
            entity.Property(e => e.ChangedColumns).HasColumnType("jsonb");
            entity.Property(e => e.PrimaryKey).HasColumnType("jsonb");
        });
    }
}
