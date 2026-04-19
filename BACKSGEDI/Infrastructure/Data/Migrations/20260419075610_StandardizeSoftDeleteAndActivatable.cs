using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeSoftDeleteAndActivatable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UsuariosRoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UsuariosRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "JefesDepartamento");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JefesDepartamento");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Coordinadores");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Coordinadores");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Alumnos");
        }
    }
}
