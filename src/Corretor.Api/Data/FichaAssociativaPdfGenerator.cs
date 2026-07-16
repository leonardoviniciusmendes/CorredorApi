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
        var font = new XFont("Arial", 8, XFontStyleEx.Regular);
        var brush = XBrushes.Black;

        DrawValue(gfx, font, brush, "X", 121, 147, 12);
        DrawValue(gfx, font, brush, dados.PessoaFisica?.Nome ?? dados.Lead.Nome, 125, 191, 320);

        if (dados.Enderecos.Count > 0)
        {
            var endereco = dados.Enderecos[0];
            DrawValue(gfx, font, brush, endereco.Logradouro, 88, 213, 360);
            DrawValue(gfx, font, brush, $"{endereco.Cidade} - {endereco.Estado}", 67, 235, 120);
            DrawValue(gfx, font, brush, FormatCep(endereco.Cep), 212, 235, 110);
        }

        DrawValue(gfx, font, brush, FormatPhone(dados.PessoaFisica?.Telefone ?? dados.Lead.Telefone), 347, 235, 120);
        DrawValue(gfx, font, brush, FormatDate(dados.PessoaFisica?.DataNascimento), 130, 270, 80);
        DrawValue(gfx, font, brush, FormatCpf(dados.PessoaFisica?.Cpf), 267, 270, 180);
        DrawValue(gfx, font, brush, FormatDate(dados.Lead.DataEnvio), 64, 686, 130);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static void DrawValue(XGraphics gfx, XFont font, XBrush brush, string? value, double x, double y, double width)
    {
        gfx.DrawString(Value(value, width), font, brush, new XRect(x, y, width, 11), XStringFormats.TopLeft);
    }

    private static string Value(string? value, double width)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var max = Math.Max(8, (int)(width / 4.1));
        var normalized = value.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= max ? normalized : normalized[..Math.Max(0, max - 3)] + "...";
    }

    private static string FormatCpf(string? value)
    {
        var digits = Digits(value);
        return digits.Length == 11
            ? $"{digits[..3]}.{digits.Substring(3, 3)}.{digits.Substring(6, 3)}-{digits[9..]}"
            : Value(value, 80);
    }

    private static string FormatCep(string? value)
    {
        var digits = Digits(value);
        return digits.Length == 8 ? $"{digits[..5]}-{digits[5..]}" : Value(value, 70);
    }

    private static string FormatPhone(string? value)
    {
        var digits = Digits(value);
        return digits.Length == 11
            ? $"({digits[..2]}) {digits.Substring(2, 5)}-{digits[7..]}"
            : digits.Length == 10
                ? $"({digits[..2]}) {digits.Substring(2, 4)}-{digits[6..]}"
                : Value(value, 100);
    }

    private static string FormatDate(string? value)
    {
        return DateTime.TryParse(value, out var date) ? date.ToString("dd/MM/yyyy") : Value(value, 90);
    }

    private static string Digits(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());
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
public sealed record FichaPessoaFisicaDados(Guid Id, string Nome, string? Cpf, string? Email, string? Telefone, string? FaixaEtaria, string? DataNascimento, string? NomeMae, string? NomePai);
public sealed record FichaPessoaJuridicaDados(Guid Id, string NomeEmpresa, string Cnpj, string IE, string? Email, string? Telefone, DateTime DataAbertura);
public sealed record FichaEnderecoDados(string Logradouro, string Estado, string Cidade, string Cep);
public sealed record FichaFaixaEtariaDados(string Faixa, int Quantidade);
public sealed record FichaDependenteDados(Guid Id, Guid PessoaFisicaId, Guid? PessoaFisicaDependenteId, string? Nome, string? Cpf, string? DataNascimento, string? NomeMae, string? NomePai);
