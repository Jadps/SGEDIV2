using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PlantillasDocumentos",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PlantillasDocumentos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DocumentosAlumnos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DocumentosAcuerdos");
        }
    }
}
