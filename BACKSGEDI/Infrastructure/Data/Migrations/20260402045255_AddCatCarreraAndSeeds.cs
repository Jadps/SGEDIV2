using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCatCarreraAndSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carreras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Clave = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carreras", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carreras");
        }
    }
}
