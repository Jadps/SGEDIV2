using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase0_RemoveAsesorFKsFromDocumentoAcuerdo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosAcuerdos_Usuarios_AsesorExternoId",
                table: "DocumentosAcuerdos");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosAcuerdos_Usuarios_AsesorInternoId",
                table: "DocumentosAcuerdos");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosAcuerdos_Usuarios_ProfesorId",
                table: "DocumentosAcuerdos");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosAcuerdos_AsesorExternoId",
                table: "DocumentosAcuerdos");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosAcuerdos_AsesorInternoId",
                table: "DocumentosAcuerdos");

            migrationBuilder.DropColumn(
                name: "AsesorExternoId",
                table: "DocumentosAcuerdos");

            migrationBuilder.DropColumn(
                name: "AsesorInternoId",
                table: "DocumentosAcuerdos");

            migrationBuilder.AddColumn<Guid>(
                name: "AsesorExternoId",
                table: "Alumnos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AsesorInternoId",
                table: "Alumnos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Alumnos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CargasAcademicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfesorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Semestre = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargasAcademicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargasAcademicas_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CargasAcademicas_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CargasAcademicas_Profesores_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Profesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_AsesorExternoId",
                table: "Alumnos",
                column: "AsesorExternoId");

            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_AsesorInternoId",
                table: "Alumnos",
                column: "AsesorInternoId");

            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_EmpresaId",
                table: "Alumnos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CargasAcademicas_AlumnoId",
                table: "CargasAcademicas",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_CargasAcademicas_MateriaId",
                table: "CargasAcademicas",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CargasAcademicas_ProfesorId",
                table: "CargasAcademicas",
                column: "ProfesorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alumnos_AsesoresExternos_AsesorExternoId",
                table: "Alumnos",
                column: "AsesorExternoId",
                principalTable: "AsesoresExternos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alumnos_AsesoresInternos_AsesorInternoId",
                table: "Alumnos",
                column: "AsesorInternoId",
                principalTable: "AsesoresInternos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alumnos_Empresas_EmpresaId",
                table: "Alumnos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosAcuerdos_Profesores_ProfesorId",
                table: "DocumentosAcuerdos",
                column: "ProfesorId",
                principalTable: "Profesores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alumnos_AsesoresExternos_AsesorExternoId",
                table: "Alumnos");

            migrationBuilder.DropForeignKey(
                name: "FK_Alumnos_AsesoresInternos_AsesorInternoId",
                table: "Alumnos");

            migrationBuilder.DropForeignKey(
                name: "FK_Alumnos_Empresas_EmpresaId",
                table: "Alumnos");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosAcuerdos_Profesores_ProfesorId",
                table: "DocumentosAcuerdos");

            migrationBuilder.DropTable(
                name: "CargasAcademicas");

            migrationBuilder.DropIndex(
                name: "IX_Alumnos_AsesorExternoId",
                table: "Alumnos");

            migrationBuilder.DropIndex(
                name: "IX_Alumnos_AsesorInternoId",
                table: "Alumnos");

            migrationBuilder.DropIndex(
                name: "IX_Alumnos_EmpresaId",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "AsesorExternoId",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "AsesorInternoId",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Alumnos");

            migrationBuilder.AddColumn<Guid>(
                name: "AsesorExternoId",
                table: "DocumentosAcuerdos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AsesorInternoId",
                table: "DocumentosAcuerdos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAcuerdos_AsesorExternoId",
                table: "DocumentosAcuerdos",
                column: "AsesorExternoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAcuerdos_AsesorInternoId",
                table: "DocumentosAcuerdos",
                column: "AsesorInternoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosAcuerdos_Usuarios_AsesorExternoId",
                table: "DocumentosAcuerdos",
                column: "AsesorExternoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosAcuerdos_Usuarios_AsesorInternoId",
                table: "DocumentosAcuerdos",
                column: "AsesorInternoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosAcuerdos_Usuarios_ProfesorId",
                table: "DocumentosAcuerdos",
                column: "ProfesorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
