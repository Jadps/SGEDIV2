using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMateriaToDocumentoAcuerdo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MateriaId",
                table: "DocumentosAcuerdos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAcuerdos_MateriaId",
                table: "DocumentosAcuerdos",
                column: "MateriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosAcuerdos_Materias_MateriaId",
                table: "DocumentosAcuerdos",
                column: "MateriaId",
                principalTable: "Materias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosAcuerdos_Materias_MateriaId",
                table: "DocumentosAcuerdos");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosAcuerdos_MateriaId",
                table: "DocumentosAcuerdos");

            migrationBuilder.DropColumn(
                name: "MateriaId",
                table: "DocumentosAcuerdos");
        }
    }
}
