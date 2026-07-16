using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class VinculaDependentePessoaFisica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "PessoaFisica",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "PessoaFisicaDependenteId",
                table: "Dependente",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Dependente_PessoaFisicaDependenteId",
                table: "Dependente",
                column: "PessoaFisicaDependenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dependente_PessoaFisica_PessoaFisicaDependenteId",
                table: "Dependente",
                column: "PessoaFisicaDependenteId",
                principalTable: "PessoaFisica",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dependente_PessoaFisica_PessoaFisicaDependenteId",
                table: "Dependente");

            migrationBuilder.DropIndex(
                name: "IX_Dependente_PessoaFisicaDependenteId",
                table: "Dependente");

            migrationBuilder.DropColumn(
                name: "PessoaFisicaDependenteId",
                table: "Dependente");

            migrationBuilder.UpdateData(
                table: "PessoaFisica",
                keyColumn: "Cpf",
                keyValue: null,
                column: "Cpf",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "PessoaFisica",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
