using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnifyEntityStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UsuariosRoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UsuariosRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Profesores");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Profesores");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PlantillasDocumentos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Materias");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "JefesDepartamento");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JefesDepartamento");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DocumentosAlumnos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DocumentosAcuerdos");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Coordinadores");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Coordinadores");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Carreras");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AsesoresInternos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AsesoresExternos");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Alumnos");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Profesores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PlantillasDocumentos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Materias",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "JefesDepartamento",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Empresas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Coordinadores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Carreras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AsesoresInternos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AsesoresExternos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Alumnos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Profesores");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PlantillasDocumentos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Materias");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "JefesDepartamento");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Coordinadores");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Carreras");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AsesoresInternos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AsesoresExternos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Alumnos");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UsuariosRoles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UsuariosRoles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Profesores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Profesores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PlantillasDocumentos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Materias",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "JefesDepartamento",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JefesDepartamento",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Empresas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DocumentosAlumnos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DocumentosAcuerdos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Coordinadores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Coordinadores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Carreras",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AsesoresInternos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AsesoresExternos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Alumnos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Alumnos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
