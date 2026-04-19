using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixInitialActiveState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"UsuariosRoles\" SET \"IsActive\" = true");
            migrationBuilder.Sql("UPDATE \"Alumnos\" SET \"IsActive\" = true");
            migrationBuilder.Sql("UPDATE \"Coordinadores\" SET \"IsActive\" = true");
            migrationBuilder.Sql("UPDATE \"JefesDepartamento\" SET \"IsActive\" = true");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
