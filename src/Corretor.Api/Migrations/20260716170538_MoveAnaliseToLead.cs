using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class MoveAnaliseToLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Simulacao");

            migrationBuilder.AddColumn<string>(
                name: "DataHoraEnvioAnalise",
                table: "Lead",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RetornoAnalise",
                table: "Lead",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TokenConsultaAnalise",
                table: "Lead",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE `Lead` SET `WorkflowEtapa` = 'Analise' WHERE `WorkflowEtapa` = 'Simulacao';");
            migrationBuilder.Sql("UPDATE `Historico` SET `Tipo` = 'EnvioAnalise' WHERE `Tipo` = 'EnvioSimulacao';");
            migrationBuilder.Sql("UPDATE `Historico` SET `Tipo` = 'AprovacaoAnalise' WHERE `Tipo` = 'AprovacaoSimulacao';");

            migrationBuilder.UpdateData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111114"),
                columns: new[] { "Etapa", "Mensagem" },
                values: new object[] { "EnvioAnalise", "Segue a analise com as opcoes de plano. Veja valores, rede, coparticipacao e cobertura antes de escolher a melhor alternativa." });

            migrationBuilder.UpdateData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111115"),
                column: "Mensagem",
                value: "Conseguiu avaliar a analise enviada? Posso te ajudar a comparar as opcoes e tirar duvidas sobre rede, carencia e valores.");

            migrationBuilder.UpdateData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111116"),
                column: "Etapa",
                value: "AprovacaoAnalise");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataHoraEnvioAnalise",
                table: "Lead");

            migrationBuilder.DropColumn(
                name: "RetornoAnalise",
                table: "Lead");

            migrationBuilder.DropColumn(
                name: "TokenConsultaAnalise",
                table: "Lead");

            migrationBuilder.Sql("UPDATE `Lead` SET `WorkflowEtapa` = 'Simulacao' WHERE `WorkflowEtapa` = 'Analise';");
            migrationBuilder.Sql("UPDATE `Historico` SET `Tipo` = 'EnvioSimulacao' WHERE `Tipo` = 'EnvioAnalise';");
            migrationBuilder.Sql("UPDATE `Historico` SET `Tipo` = 'AprovacaoSimulacao' WHERE `Tipo` = 'AprovacaoAnalise';");

            migrationBuilder.CreateTable(
                name: "Simulacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Aprovada = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataEnvio = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LeadId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Link = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Simulacao_Lead_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111114"),
                columns: new[] { "Etapa", "Mensagem" },
                values: new object[] { "EnvioSimulacao", "Segue a simulacao com as opcoes de plano. Veja valores, rede, coparticipacao e cobertura antes de escolher a melhor alternativa." });

            migrationBuilder.UpdateData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111115"),
                column: "Mensagem",
                value: "Conseguiu avaliar a simulacao enviada? Posso te ajudar a comparar as opcoes e tirar duvidas sobre rede, carencia e valores.");

            migrationBuilder.UpdateData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111116"),
                column: "Etapa",
                value: "AprovacaoSimulacao");

            migrationBuilder.CreateIndex(
                name: "IX_Simulacao_LeadId",
                table: "Simulacao",
                column: "LeadId");
        }
    }
}
