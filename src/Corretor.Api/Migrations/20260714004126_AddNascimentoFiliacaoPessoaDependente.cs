using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNascimentoFiliacaoPessoaDependente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataNascimento",
                table: "PessoaFisica",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NomeMae",
                table: "PessoaFisica",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NomePai",
                table: "PessoaFisica",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DataNascimento",
                table: "Dependente",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NomeMae",
                table: "Dependente",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NomePai",
                table: "Dependente",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "PessoaFisica");

            migrationBuilder.DropColumn(
                name: "NomeMae",
                table: "PessoaFisica");

            migrationBuilder.DropColumn(
                name: "NomePai",
                table: "PessoaFisica");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Dependente");

            migrationBuilder.DropColumn(
                name: "NomeMae",
                table: "Dependente");

            migrationBuilder.DropColumn(
                name: "NomePai",
                table: "Dependente");
        }
    }
}
