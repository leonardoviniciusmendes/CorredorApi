using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDatasToLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataAprovacao",
                table: "Lead",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DataEnvio",
                table: "Lead",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DataRetorno",
                table: "Lead",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataAprovacao",
                table: "Lead");

            migrationBuilder.DropColumn(
                name: "DataEnvio",
                table: "Lead");

            migrationBuilder.DropColumn(
                name: "DataRetorno",
                table: "Lead");
        }
    }
}
