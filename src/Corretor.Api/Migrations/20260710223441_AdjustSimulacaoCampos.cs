using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdjustSimulacaoCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Operadora",
                table: "Simulacao");

            migrationBuilder.DropColumn(
                name: "Valor",
                table: "Simulacao");

            migrationBuilder.DropColumn(
                name: "Plano",
                table: "Simulacao");

            migrationBuilder.AddColumn<string>(
                name: "DataEnvio",
                table: "Simulacao",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataEnvio",
                table: "Simulacao");

            migrationBuilder.AddColumn<string>(
                name: "Plano",
                table: "Simulacao",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Operadora",
                table: "Simulacao",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Valor",
                table: "Simulacao",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
