using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Corretor.Api.Data;

public static class FichaAssociativaPdfGenerator
{
    public static byte[] Gerar(FichaAssociativaDados dados, string templatePath)
    {
        GlobalFontSettings.UseWindowsFontsUnderWindows = true;

        using var document = PdfReader.Open(templatePath, PdfDocumentOpenMode.Modify);
        var page = document.Pages[0];

        using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
        var titleFont = new XFont("Arial", 10, XFontStyleEx.Bold);
        var labelFont = new XFont("Arial", 7.5, XFontStyleEx.Bold);
        var valueFont = new XFont("Arial", 8, XFontStyleEx.Regular);
        var brush = XBrushes.Black;

        var y = 76d;
        DrawSectionTitle(gfx, titleFont, brush, "DADOS PREENCHIDOS PELO SISTEMA", 42, y);
        y += 20;

        DrawField(gfx, labelFont, valueFont, brush, "Nome", dados.PessoaFisica?.Nome ?? dados.Lead.Nome, 42, y, 260);
        DrawField(gfx, labelFont, valueFont, brush, "CPF", dados.PessoaFisica?.Cpf, 330, y, 150);
        y += 18;

        DrawField(gfx, labelFont, valueFont, brush, "Telefone", dados.PessoaFisica?.Telefone ?? dados.Lead.Telefone, 42, y, 150);
        DrawField(gfx, labelFont, valueFont, brush, "Email", dados.PessoaFisica?.Email ?? dados.Lead.Email, 220, y, 260);
        y += 18;

        DrawField(gfx, labelFont, valueFont, brush, "Operadora", dados.Lead.Operadora, 42, y, 150);
        DrawField(gfx, labelFont, valueFont, brush, "Quantidade de vidas", dados.Lead.QuantidadeVidas.ToString(), 220, y, 90);
        DrawField(gfx, labelFont, valueFont, brush, "Faixa etaria", dados.PessoaFisica?.FaixaEtaria, 340, y, 140);
        y += 18;

        if (dados.Enderecos.Count > 0)
        {
            var endereco = dados.Enderecos[0];
            DrawField(gfx, labelFont, valueFont, brush, "Endereco", endereco.Logradouro, 42, y, 260);
            DrawField(gfx, labelFont, valueFont, brush, "Cidade/UF", $"{endereco.Cidade} - {endereco.Estado}", 330, y, 150);
            y += 18;
            DrawField(gfx, labelFont, valueFont, brush, "CEP", endereco.Cep, 42, y, 120);
            y += 18;
        }

        if (dados.PessoaJuridica is not null)
        {
            DrawSectionTitle(gfx, titleFont, brush, "DADOS DA EMPRESA", 42, y);
            y += 18;
            DrawField(gfx, labelFont, valueFont, brush, "Empresa", dados.PessoaJuridica.NomeEmpresa, 42, y, 260);
            DrawField(gfx, labelFont, valueFont, brush, "CNPJ", dados.PessoaJuridica.Cnpj, 330, y, 150);
            y += 18;
            DrawField(gfx, labelFont, valueFont, brush, "IE", dados.PessoaJuridica.IE, 42, y, 120);
            DrawField(gfx, labelFont, valueFont, brush, "Telefone", dados.PessoaJuridica.Telefone, 220, y, 120);
            DrawField(gfx, labelFont, valueFont, brush, "Email", dados.PessoaJuridica.Email, 340, y, 150);
            y += 20;
        }

        if (dados.FaixasEtarias.Count > 0)
        {
            DrawSectionTitle(gfx, titleFont, brush, "VIDAS POR FAIXA ETARIA", 42, y);
            y += 18;

            foreach (var faixa in dados.FaixasEtarias.Take(8))
            {
                DrawField(gfx, labelFont, valueFont, brush, faixa.Faixa, faixa.Quantidade.ToString(), 42, y, 160);
                y += 14;
            }
        }

        DrawField(gfx, labelFont, valueFont, brush, "Data de envio", dados.Lead.DataEnvio, 42, 720, 120);
        DrawField(gfx, labelFont, valueFont, brush, "Data de retorno", dados.Lead.DataRetorno, 190, 720, 120);
        DrawField(gfx, labelFont, valueFont, brush, "Data de aprovacao", dados.Lead.DataAprovacao, 340, 720, 140);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static void DrawSectionTitle(XGraphics gfx, XFont font, XBrush brush, string text, double x, double y)
    {
        gfx.DrawString(text, font, brush, new XRect(x, y, 500, 12), XStringFormats.TopLeft);
    }

    private static void DrawField(XGraphics gfx, XFont labelFont, XFont valueFont, XBrush brush, string label, string? value, double x, double y, double width)
    {
        gfx.DrawString($"{label}:", labelFont, brush, new XRect(x, y, width, 10), XStringFormats.TopLeft);
        gfx.DrawString(Value(value), valueFont, brush, new XRect(x, y + 9, width, 10), XStringFormats.TopLeft);
    }

    private static string Value(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
}

public sealed record FichaAssociativaDados(
    FichaLeadDados Lead,
    FichaPessoaFisicaDados? PessoaFisica,
    FichaPessoaJuridicaDados? PessoaJuridica,
    List<FichaEnderecoDados> Enderecos,
    List<FichaFaixaEtariaDados> FaixasEtarias,
    List<FichaDependenteDados> Dependentes);

public sealed record FichaLeadDados(string Nome, string Telefone, int QuantidadeVidas, string? Operadora, string? Email, string? DataEnvio, string? DataRetorno, string? DataAprovacao);
public sealed record FichaPessoaFisicaDados(Guid Id, string Nome, string Cpf, string? Email, string? Telefone, string? FaixaEtaria);
public sealed record FichaPessoaJuridicaDados(Guid Id, string NomeEmpresa, string Cnpj, string IE, string? Email, string? Telefone, DateTime DataAbertura);
public sealed record FichaEnderecoDados(string Logradouro, string Estado, string Cidade, string Cep);
public sealed record FichaFaixaEtariaDados(string Faixa, int Quantidade);
public sealed record FichaDependenteDados(Guid Id, Guid PessoaFisicaId);
