using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BACKSGEDI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestoreContratoRequiredRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContratosProfesores_Profesores_ProfesorId",
                table: "ContratosProfesores");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfesorId",
                table: "ContratosProfesores",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MateriaId",
                table: "ContratosProfesores",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AlumnoId",
                table: "ContratosProfesores",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ContratosProfesores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ContratosProfesores_Profesores_ProfesorId",
                table: "ContratosProfesores",
                column: "ProfesorId",
                principalTable: "Profesores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContratosProfesores_Profesores_ProfesorId",
                table: "ContratosProfesores");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ContratosProfesores");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProfesorId",
                table: "ContratosProfesores",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "MateriaId",
                table: "ContratosProfesores",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "AlumnoId",
                table: "ContratosProfesores",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ContratosProfesores_Profesores_ProfesorId",
                table: "ContratosProfesores",
                column: "ProfesorId",
                principalTable: "Profesores",
                principalColumn: "Id");
        }
    }
}
