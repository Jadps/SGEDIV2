using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinadorJefeDept : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Coordinadores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CarreraId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coordinadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coordinadores_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coordinadores_carreras_CarreraId",
                        column: x => x.CarreraId,
                        principalTable: "carreras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JefesDepartamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CarreraId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JefesDepartamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JefesDepartamento_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JefesDepartamento_carreras_CarreraId",
                        column: x => x.CarreraId,
                        principalTable: "carreras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coordinadores_CarreraId",
                table: "Coordinadores",
                column: "CarreraId");

            migrationBuilder.CreateIndex(
                name: "IX_Coordinadores_UsuarioId",
                table: "Coordinadores",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JefesDepartamento_CarreraId",
                table: "JefesDepartamento",
                column: "CarreraId");

            migrationBuilder.CreateIndex(
                name: "IX_JefesDepartamento_UsuarioId",
                table: "JefesDepartamento",
                column: "UsuarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coordinadores");

            migrationBuilder.DropTable(
                name: "JefesDepartamento");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Usuarios");

            migrationBuilder.InsertData(
                table: "carreras",
                columns: new[] { "Id", "Clave", "IsDeleted", "Nombre" },
                values: new object[,]
                {
                    { 1, "IGE", false, "Ingeniería en Gestión Empresarial" },
                    { 2, "ITIC", false, "Ingeniería en Tecnologías de la Información y Comunicaciones" },
                    { 3, "II", false, "Ingeniería Industrial" },
                    { 4, "IA", false, "Ingeniería en Administración" },
                    { 5, "IDA", false, "Ingeniería en Desarrollo de Aplicaciones" },
                    { 6, "IE", false, "Ingeniería Eléctrica" },
                    { 7, "IEM", false, "Ingeniería Electromecánica" },
                    { 8, "IEL", false, "Ingeniería Electrónica" },
                    { 9, "IM", false, "Ingeniería Mecánica" },
                    { 10, "IMT", false, "Ingeniería Mecatrónica" }
                });
        }
    }
}
