using Corretor.Api.Controllers;
using Microsoft.Extensions.Options;

namespace Corretor.Api.Data;

public sealed class ExternalDocumentosClient(HttpClient httpClient, IOptions<ExternalDocumentosOptions> options)
{
    private readonly ExternalDocumentosOptions _options = options.Value;

    public async Task EnviarDocumento(DocumentoUploadRequest request, string filePath, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        using var form = new MultipartFormDataContent();
        AddString(form, "cpf", request.Cpf);
        AddString(form, "cpfDependente", request.CpfDependente);
        AddString(form, "cnpj", request.Cnpj);
        AddString(form, "observacoes", request.Observacoes);
        AddString(form, "papel", request.Papel);
        AddString(form, "tipoParentesco", request.TipoParentesco);
        AddString(form, "tipo", request.Tipo);

        await using var fileStream = File.OpenRead(filePath);
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(request.Arquivo.ContentType) ? "application/octet-stream" : request.Arquivo.ContentType);

        form.Add(fileContent, "arquivo", request.Arquivo.FileName);

        using var response = await httpClient.PostAsync("/api/Documentos", form, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static void AddString(MultipartFormDataContent form, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            form.Add(new StringContent(value), name);
        }
    }
}
