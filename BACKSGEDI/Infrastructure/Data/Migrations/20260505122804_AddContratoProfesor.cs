using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContratoProfesor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContratosProfesores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CarreraId = table.Column<int>(type: "integer", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfesorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Modalidad = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MotivoRechazo = table.Column<string>(type: "text", nullable: true),
                    FechaAceptacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratosProfesores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratosProfesores_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContratosProfesores_Carreras_CarreraId",
                        column: x => x.CarreraId,
                        principalTable: "Carreras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContratosProfesores_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContratosProfesores_Profesores_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Profesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriteriosEvaluacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContratoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Detalle = table.Column<string>(type: "text", nullable: false),
                    Porcentaje = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriosEvaluacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriosEvaluacion_ContratosProfesores_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "ContratosProfesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContratosProfesores_AlumnoId",
                table: "ContratosProfesores",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratosProfesores_CarreraId",
                table: "ContratosProfesores",
                column: "CarreraId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratosProfesores_MateriaId",
                table: "ContratosProfesores",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratosProfesores_ProfesorId",
                table: "ContratosProfesores",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriosEvaluacion_ContratoId",
                table: "CriteriosEvaluacion",
                column: "ContratoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CriteriosEvaluacion");

            migrationBuilder.DropTable(
                name: "ContratosProfesores");
        }
    }
}
