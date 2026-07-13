using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Corretor.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedScriptsPorEtapa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Script",
                columns: new[] { "Id", "Etapa", "Mensagem", "Tipo" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "CriacaoLead", "Ola, recebi seu contato para cotacao de plano de saude. Vou confirmar alguns dados para encontrar as melhores opcoes.", "WhatsApp" },
                    { new Guid("11111111-1111-1111-1111-111111111112"), "ConversaInicial", "Voce procura plano individual, familiar ou empresarial? Tambem preciso saber cidade, operadora atual e se ha preferencia de rede.", "PerguntasIniciais" },
                    { new Guid("11111111-1111-1111-1111-111111111113"), "FaixaEtaria", "Para calcular corretamente, me envie a quantidade de vidas por faixa etaria e informe quem sera titular e quem sera dependente.", "FaixaEtaria" },
                    { new Guid("11111111-1111-1111-1111-111111111114"), "EnvioSimulacao", "Segue a simulacao com as opcoes de plano. Veja valores, rede, coparticipacao e cobertura antes de escolher a melhor alternativa.", "Envio" },
                    { new Guid("11111111-1111-1111-1111-111111111115"), "RetornoContato", "Conseguiu avaliar a simulacao enviada? Posso te ajudar a comparar as opcoes e tirar duvidas sobre rede, carencia e valores.", "FollowUp" },
                    { new Guid("11111111-1111-1111-1111-111111111116"), "AprovacaoSimulacao", "Perfeito, vamos seguir com a opcao escolhida. Vou iniciar a etapa de documentos para formalizar a proposta.", "Aprovacao" },
                    { new Guid("11111111-1111-1111-1111-111111111117"), "Documentacao", "Envie os documentos do titular, dependentes ou empresa conforme o caso, incluindo identificacao e comprovante de endereco.", "Documentos" },
                    { new Guid("11111111-1111-1111-1111-111111111118"), "Contrato", "A proposta esta pronta para assinatura. Confira os dados do contrato e me avise quando concluir para acompanharmos a implantacao.", "Assinatura" },
                    { new Guid("11111111-1111-1111-1111-111111111119"), "PosContrato", "Contrato concluido. Vou acompanhar os proximos passos e te orientar sobre carteirinha, acesso ao aplicativo e uso do plano.", "BoasVindas" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111112"));

            migrationBuilder.DeleteData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111113"));

            migrationBuilder.DeleteData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111114"));

            migrationBuilder.DeleteData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111115"));

            migrationBuilder.DeleteData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111116"));

            migrationBuilder.DeleteData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111117"));

            migrationBuilder.DeleteData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111118"));

            migrationBuilder.DeleteData(
                table: "Script",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111119"));
        }
    }
}
