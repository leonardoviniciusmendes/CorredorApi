using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDocumentoTipos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "Dependentes",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "Identificacao",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Documento");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Documento",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DataUpload",
                table: "Documento",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DocumentoDe",
                table: "Documento",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "DataUpload",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "DocumentoDe",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "TipoIdentificacao",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "TipoEndereco",
                table: "Documento");

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Dependentes",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Identificacao",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
