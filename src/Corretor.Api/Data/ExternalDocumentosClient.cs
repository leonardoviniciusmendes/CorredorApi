using Microsoft.Extensions.Options;

namespace Corretor.Api.Data;

public sealed class ExternalDocumentosClient(HttpClient httpClient, IOptions<ExternalDocumentosOptions> options)
{
    private readonly ExternalDocumentosOptions _options = options.Value;

    public async Task<HttpResponseMessage> Reprocessar(Guid documentoExternoId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new HttpRequestException("External document API base URL is not configured.");
        }

        return await httpClient.PostAsync($"/api/Documentos/{documentoExternoId}/extrair-identificacao", null, cancellationToken);
    }

    public async Task<HttpResponseMessage> ObterIdentificacao(Guid documentoExternoId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new HttpRequestException("External document API base URL is not configured.");
        }

        return await httpClient.GetAsync($"/api/Documentos/{documentoExternoId}/identificacao", cancellationToken);
    }
}
