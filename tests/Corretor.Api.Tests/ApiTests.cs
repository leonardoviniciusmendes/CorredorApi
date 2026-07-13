using System.Net;
using System.Net.Http.Json;

namespace Corretor.Api.Tests;

public sealed class ApiTests : IClassFixture<CorretorApiFactory>
{
    private readonly HttpClient _client;

    public ApiTests(CorretorApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostLead_CreatesLead()
    {
        var response = await CreateLead();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetLead_WhenMissing_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/leads/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostFaixaEtaria_WhenLeadMissing_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync($"/api/leads/{Guid.NewGuid()}/faixas-etarias", new
        {
            faixa = "29-33",
            quantidade = 1
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostSimulacao_CreatesSimulacao()
    {
        var lead = await CreateLeadResponse();

        var response = await _client.PostAsJsonAsync($"/api/leads/{lead.Id}/simulacoes", new
        {
            link = "https://example.com",
            aprovada = false
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var simulacao = await response.Content.ReadFromJsonAsync<SimulacaoTestResponse>();
        Assert.NotNull(simulacao);
        Assert.False(string.IsNullOrWhiteSpace(simulacao.DataEnvio));

        var leadAtualizado = await _client.GetFromJsonAsync<LeadTestResponse>($"/api/leads/{lead.Id}");
        Assert.NotNull(leadAtualizado);
        Assert.Equal("Simulacao", leadAtualizado.WorkflowEtapa);
    }

    [Fact]
    public async Task PostSimulacaoAprovada_UnapprovesPreviousSimulation()
    {
        var lead = await CreateLeadResponse();

        var primeira = await CreateSimulacaoResponse(lead.Id, true);
        var segunda = await CreateSimulacaoResponse(lead.Id, true);

        var primeiraAtualizada = await _client.GetFromJsonAsync<SimulacaoTestResponse>($"/api/simulacoes/{primeira.Id}");
        var segundaAtualizada = await _client.GetFromJsonAsync<SimulacaoTestResponse>($"/api/simulacoes/{segunda.Id}");

        Assert.NotNull(primeiraAtualizada);
        Assert.NotNull(segundaAtualizada);
        Assert.False(primeiraAtualizada.Aprovada);
        Assert.True(segundaAtualizada.Aprovada);
    }

    [Fact]
    public async Task PostCliente_CreatesClienteLinkedToLead()
    {
        var lead = await CreateLeadResponse();
        var pessoaFisica = await CreatePessoaFisicaResponse();

        var response = await _client.PostAsJsonAsync("/api/clientes", new
        {
            leadId = lead.Id,
            pessoaFisicaId = pessoaFisica.Id
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostDocumentoIdentificacao_CreatesDocumento()
    {
        var lead = await CreateLeadResponse();
        var documentoExternoId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync($"/api/leads/{lead.Id}/documentos", new
        {
            documentoExternoId,
            tipo = "CNH",
            papel = "Titular",
            tipoParentesco = "Titular",
            cpf = "12345678901",
            cpfDependente = (string?)null,
            cnpj = (string?)null,
            extracaoProcessada = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var documento = await response.Content.ReadFromJsonAsync<DocumentoTestResponse>();
        Assert.NotNull(documento);
        Assert.Equal(documentoExternoId, documento.DocumentoExternoId);
        Assert.Equal("CNH", documento.Tipo);
        Assert.Equal("Titular", documento.Papel);
        Assert.True(documento.ExtracaoProcessada);
        Assert.False(documento.Aprovado);

        var aprovacaoResponse = await _client.PostAsync($"/api/documentos/{documento.Id}/aprovar", null);
        Assert.Equal(HttpStatusCode.OK, aprovacaoResponse.StatusCode);

        var aprovacao = await aprovacaoResponse.Content.ReadFromJsonAsync<DocumentoAprovacaoTestResponse>();
        Assert.NotNull(aprovacao);
        Assert.True(aprovacao.Aprovado);
        Assert.False(string.IsNullOrWhiteSpace(aprovacao.DataAprovacao));

        var deleteResponse = await _client.DeleteAsync($"/api/documentos/{documento.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task PostLead_CreatesCadastroLeadHistorico()
    {
        var lead = await CreateLeadResponse();

        var historicos = await _client.GetFromJsonAsync<List<HistoricoTestResponse>>($"/api/leads/{lead.Id}/historicos");

        Assert.NotNull(historicos);
        Assert.Contains(historicos, x => x.Tipo == "CadastroLead");
    }

    [Fact]
    public async Task GetScriptsByEtapa_ReturnsSeededScripts()
    {
        var scripts = await _client.GetFromJsonAsync<List<ScriptTestResponse>>("/api/scripts?etapa=FaixaEtaria");

        Assert.NotNull(scripts);
        Assert.Contains(scripts, x => x.Etapa == "FaixaEtaria");
    }

    [Fact]
    public async Task GetFichaAssociativa_ReturnsPdf()
    {
        var lead = await CreateLeadResponse();

        var response = await _client.GetAsync($"/api/leads/{lead.Id}/ficha-associativa/pdf");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 4);
        Assert.Equal("%PDF"u8.ToArray(), bytes[..4]);
    }

    private Task<HttpResponseMessage> CreateLead()
    {
        return _client.PostAsJsonAsync("/api/leads", new
        {
            nome = "Maria Silva",
            telefone = "11999999999",
            quantidadeVidas = 3,
            operadora = "Operadora A",
            email = "maria@example.com",
            dataEnvio = "2026-07-10",
            dataRetorno = "2026-07-11",
            dataAprovacao = "2026-07-12"
        });
    }

    private async Task<LeadTestResponse> CreateLeadResponse()
    {
        var response = await CreateLead();
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LeadTestResponse>()
            ?? throw new InvalidOperationException("Lead response was empty.");
    }

    private async Task<PessoaFisicaTestResponse> CreatePessoaFisicaResponse()
    {
        var response = await _client.PostAsJsonAsync("/api/pessoas-fisicas", new
        {
            nome = "Maria Silva",
            cpf = "12345678900",
            email = "maria@example.com",
            telefone = "11999999999",
            faixaEtaria = "34-38"
        });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PessoaFisicaTestResponse>()
            ?? throw new InvalidOperationException("Pessoa fisica response was empty.");
    }

    private async Task<SimulacaoTestResponse> CreateSimulacaoResponse(Guid leadId, bool aprovada)
    {
        var response = await _client.PostAsJsonAsync($"/api/leads/{leadId}/simulacoes", new
        {
            link = "https://example.com",
            aprovada
        });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SimulacaoTestResponse>()
            ?? throw new InvalidOperationException("Simulacao response was empty.");
    }

    private sealed record LeadTestResponse(Guid Id, string Nome, string Telefone, int QuantidadeVidas, string? Operadora, string? Email, string? DataEnvio, string? DataRetorno, string? DataAprovacao, string WorkflowEtapa);
    private sealed record PessoaFisicaTestResponse(Guid Id, string Nome, string Cpf, string? Email, string? Telefone, string? FaixaEtaria);
    private sealed record SimulacaoTestResponse(Guid Id, Guid LeadId, string? Link, bool Aprovada, string DataEnvio);
    private sealed record DocumentoTestResponse(Guid Id, Guid LeadId, Guid DocumentoExternoId, string Tipo, string Papel, string? TipoParentesco, string? Cpf, string? CpfDependente, string? Cnpj, bool ExtracaoProcessada, bool Aprovado, string DataUpload, string? DataAprovacao, string? MotivoReprovacao);
    private sealed record DocumentoAprovacaoTestResponse(Guid Id, Guid DocumentoExternoId, bool Aprovado, string? DataAprovacao);
    private sealed record HistoricoTestResponse(Guid Id, Guid LeadId, string Tipo, string Data);
    private sealed record ScriptTestResponse(Guid Id, string Etapa, string Tipo, string Mensagem);
}
