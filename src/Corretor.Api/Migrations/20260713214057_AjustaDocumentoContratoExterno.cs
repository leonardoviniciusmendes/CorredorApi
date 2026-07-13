using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AjustaDocumentoContratoExterno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaminhoArquivo",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "DocumentoDe",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "TamanhoBytes",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "TipoIdentificacao",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "TipoEndereco",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "NomeArquivoArmazenado",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "NomeArquivo",
                table: "Documento");

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CpfDependente",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentoExternoId",
                table: "Documento",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "ExtracaoProcessada",
                table: "Documento",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Papel",
                table: "Documento",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Documento",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TipoParentesco",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "CpfDependente",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "DocumentoExternoId",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "ExtracaoProcessada",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "Papel",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "TipoParentesco",
                table: "Documento");

            migrationBuilder.AddColumn<string>(
                name: "CaminhoArquivo",
                table: "Documento",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Documento",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Documento",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DocumentoDe",
                table: "Documento",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "TamanhoBytes",
                table: "Documento",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "TipoEndereco",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TipoIdentificacao",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NomeArquivo",
                table: "Documento",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NomeArquivoArmazenado",
                table: "Documento",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
