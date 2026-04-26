using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNombreFromPlantilla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "PlantillasDocumentos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "PlantillasDocumentos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
