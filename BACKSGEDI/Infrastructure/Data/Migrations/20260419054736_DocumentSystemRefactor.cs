using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DocumentSystemRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsVersionActual",
                table: "DocumentosAlumnos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "DocumentosAlumnos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRevision",
                table: "DocumentosAlumnos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "DocumentosAlumnos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevisadoPorUsuarioId",
                table: "DocumentosAlumnos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubidoPorUsuarioId",
                table: "DocumentosAlumnos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "DocumentosAlumnos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DocumentosAcuerdos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfesorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsesorInternoId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsesorExternoId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipoAcuerdo = table.Column<int>(type: "integer", nullable: false),
                    Semestre = table.Column<string>(type: "text", nullable: false),
                    RutaArchivo = table.Column<string>(type: "text", nullable: true),
                    FechaSubida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaLimite = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MotivoRechazo = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EsVersionActual = table.Column<bool>(type: "boolean", nullable: false),
                    SubidoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaRevision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosAcuerdos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosAcuerdos_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentosAcuerdos_Usuarios_AsesorExternoId",
                        column: x => x.AsesorExternoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosAcuerdos_Usuarios_AsesorInternoId",
                        column: x => x.AsesorInternoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosAcuerdos_Usuarios_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlantillasDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoDocumento = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    RutaArchivo = table.Column<string>(type: "text", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubidaPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    EsVigente = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasDocumentos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAcuerdos_AlumnoId",
                table: "DocumentosAcuerdos",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAcuerdos_AsesorExternoId",
                table: "DocumentosAcuerdos",
                column: "AsesorExternoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAcuerdos_AsesorInternoId",
                table: "DocumentosAcuerdos",
                column: "AsesorInternoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAcuerdos_ProfesorId",
                table: "DocumentosAcuerdos",
                column: "ProfesorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosAcuerdos");

            migrationBuilder.DropTable(
                name: "PlantillasDocumentos");

            migrationBuilder.DropColumn(
                name: "EsVersionActual",
                table: "DocumentosAlumnos");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "DocumentosAlumnos");

            migrationBuilder.DropColumn(
                name: "FechaRevision",
                table: "DocumentosAlumnos");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "DocumentosAlumnos");

            migrationBuilder.DropColumn(
                name: "RevisadoPorUsuarioId",
                table: "DocumentosAlumnos");

            migrationBuilder.DropColumn(
                name: "SubidoPorUsuarioId",
                table: "DocumentosAlumnos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "DocumentosAlumnos");
        }
    }
}
