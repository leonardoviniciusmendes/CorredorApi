using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAprovadaToSimulacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Aprovada",
                table: "Simulacao",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aprovada",
                table: "Simulacao");
        }
    }
}
