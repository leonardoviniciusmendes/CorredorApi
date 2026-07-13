using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAprovacaoToDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Aprovado",
                table: "Documento",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DataAprovacao",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MotivoReprovacao",
                table: "Documento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aprovado",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "DataAprovacao",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "MotivoReprovacao",
                table: "Documento");
        }
    }
}
