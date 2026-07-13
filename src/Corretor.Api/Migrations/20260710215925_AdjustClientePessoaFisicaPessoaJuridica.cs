using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdjustClientePessoaFisicaPessoaJuridica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dependente_Pessoa_PessoaId",
                table: "Dependente");

            migrationBuilder.DropTable(
                name: "Juridica");

            migrationBuilder.DropTable(
                name: "Pessoa");

            migrationBuilder.RenameColumn(
                name: "PessoaId",
                table: "Dependente",
                newName: "PessoaFisicaId");

            migrationBuilder.RenameIndex(
                name: "IX_Dependente_PessoaId",
                table: "Dependente",
                newName: "IX_Dependente_PessoaFisicaId");

            migrationBuilder.AddColumn<Guid>(
                name: "PessoaFisicaId",
                table: "Cliente",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "PessoaJuridicaId",
                table: "Cliente",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "PessoaFisica",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cpf = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FaixaEtaria = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PessoaFisica", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PessoaJuridica",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NomeEmpresa = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cnpj = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IE = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataAbertura = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PessoaJuridica", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_PessoaFisicaId",
                table: "Cliente",
                column: "PessoaFisicaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_PessoaJuridicaId",
                table: "Cliente",
                column: "PessoaJuridicaId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cliente_PessoaFisicaOuPessoaJuridica",
                table: "Cliente",
                sql: "((`PessoaFisicaId` IS NOT NULL AND `PessoaJuridicaId` IS NULL) OR (`PessoaFisicaId` IS NULL AND `PessoaJuridicaId` IS NOT NULL))");

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_PessoaFisica_PessoaFisicaId",
                table: "Cliente",
                column: "PessoaFisicaId",
                principalTable: "PessoaFisica",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_PessoaJuridica_PessoaJuridicaId",
                table: "Cliente",
                column: "PessoaJuridicaId",
                principalTable: "PessoaJuridica",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dependente_PessoaFisica_PessoaFisicaId",
                table: "Dependente",
                column: "PessoaFisicaId",
                principalTable: "PessoaFisica",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_PessoaFisica_PessoaFisicaId",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_PessoaJuridica_PessoaJuridicaId",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Dependente_PessoaFisica_PessoaFisicaId",
                table: "Dependente");

            migrationBuilder.DropTable(
                name: "PessoaFisica");

            migrationBuilder.DropTable(
                name: "PessoaJuridica");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_PessoaFisicaId",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_PessoaJuridicaId",
                table: "Cliente");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cliente_PessoaFisicaOuPessoaJuridica",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "PessoaFisicaId",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "PessoaJuridicaId",
                table: "Cliente");

            migrationBuilder.RenameColumn(
                name: "PessoaFisicaId",
                table: "Dependente",
                newName: "PessoaId");

            migrationBuilder.RenameIndex(
                name: "IX_Dependente_PessoaFisicaId",
                table: "Dependente",
                newName: "IX_Dependente_PessoaId");

            migrationBuilder.CreateTable(
                name: "Juridica",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Juridica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Juridica_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pessoa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Cpf = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FaixaEtaria = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pessoa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pessoa_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Juridica_ClienteId",
                table: "Juridica",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoa_ClienteId",
                table: "Pessoa",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dependente_Pessoa_PessoaId",
                table: "Dependente",
                column: "PessoaId",
                principalTable: "Pessoa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
