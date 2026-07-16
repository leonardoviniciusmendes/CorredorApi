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
    public async Task PostAnalise_StoresAnaliseOnLead()
    {
        var lead = await CreateLeadResponse();

        var response = await _client.PostAsJsonAsync($"/api/leads/{lead.Id}/analise", new
        {
            tokenConsulta = "consulta-123",
            retornoAnalise = new { status = "ok" }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var analise = await response.Content.ReadFromJsonAsync<LeadAnaliseTestResponse>();
        Assert.NotNull(analise);
        Assert.Equal(lead.Id, analise.LeadId);
        Assert.Equal("consulta-123", analise.TokenConsulta);
        Assert.Equal("{\"status\":\"ok\"}", analise.RetornoAnalise);
        Assert.False(string.IsNullOrWhiteSpace(analise.DataHoraEnvioAnalise));

        var leadAtualizado = await _client.GetFromJsonAsync<LeadTestResponse>($"/api/leads/{lead.Id}");
        Assert.NotNull(leadAtualizado);
        Assert.Equal("Analise", leadAtualizado.WorkflowEtapa);
        Assert.Equal("consulta-123", leadAtualizado.TokenConsultaAnalise);
        Assert.Equal("{\"status\":\"ok\"}", leadAtualizado.RetornoAnalise);
        Assert.False(string.IsNullOrWhiteSpace(leadAtualizado.DataHoraEnvioAnalise));
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
    public async Task PostDocumentoDependente_WhenCpfDependenteMissing_CreatesDependente()
    {
        var lead = await CreateLeadResponse();
        var pessoaFisica = await CreatePessoaFisicaResponse();
        await CreateClienteResponse(lead.Id, pessoaFisica.Id);

        var response = await _client.PostAsJsonAsync($"/api/leads/{lead.Id}/documentos", new
        {
            documentoExternoId = Guid.NewGuid(),
            tipo = "RG",
            papel = "Dependente",
            tipoParentesco = "Filho",
            cpf = "12345678901",
            cpfDependente = (string?)null,
            cnpj = (string?)null,
            extracaoProcessada = true,
            nome = "Joao Silva",
            dataNascimento = "2018-05-10",
            nomeMae = "Maria Silva",
            nomePai = "Jose Silva"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var dependentes = await _client.GetFromJsonAsync<List<DependenteTestResponse>>($"/api/pessoas-fisicas/{pessoaFisica.Id}/dependentes");
        Assert.NotNull(dependentes);

        var dependente = Assert.Single(dependentes);
        Assert.NotNull(dependente.PessoaFisicaDependenteId);
        Assert.Equal("Joao Silva", dependente.Nome);
        Assert.Null(dependente.Cpf);
        Assert.Equal("Filho", dependente.TipoParentesco);
        Assert.Equal("2018-05-10", dependente.DataNascimento);
        Assert.Equal("Maria Silva", dependente.NomeMae);
        Assert.Equal("Jose Silva", dependente.NomePai);

        var pessoaDependente = await _client.GetFromJsonAsync<PessoaFisicaTestResponse>($"/api/pessoas-fisicas/{dependente.PessoaFisicaDependenteId}");
        Assert.NotNull(pessoaDependente);
        Assert.Equal("Joao Silva", pessoaDependente.Nome);
        Assert.Null(pessoaDependente.Cpf);
        Assert.Equal("2018-05-10", pessoaDependente.DataNascimento);
        Assert.Equal("Maria Silva", pessoaDependente.NomeMae);
    }

    [Fact]
    public async Task PostDocumentoDependente_CreatesDocumento()
    {
        var lead = await CreateLeadResponse();
        var pessoaFisica = await CreatePessoaFisicaResponse();
        await CreateClienteResponse(lead.Id, pessoaFisica.Id);
        var documentoExternoId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync($"/api/leads/{lead.Id}/documentos", new
        {
            documentoExternoId,
            tipo = "RG",
            papel = "Dependente",
            tipoParentesco = "Filho",
            cpf = "12345678901",
            cpfDependente = "98765432100",
            cnpj = (string?)null,
            extracaoProcessada = true,
            nome = "Joao Silva",
            dataNascimento = "2018-05-10",
            nomeMae = "Maria Silva",
            nomePai = "Jose Silva"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var documento = await response.Content.ReadFromJsonAsync<DocumentoTestResponse>();
        Assert.NotNull(documento);
        Assert.Equal(documentoExternoId, documento.DocumentoExternoId);
        Assert.Equal("Dependente", documento.Papel);
        Assert.Equal("Filho", documento.TipoParentesco);
        Assert.Equal("12345678901", documento.Cpf);
        Assert.Equal("98765432100", documento.CpfDependente);

        var dependentes = await _client.GetFromJsonAsync<List<DependenteTestResponse>>($"/api/pessoas-fisicas/{pessoaFisica.Id}/dependentes");
        Assert.NotNull(dependentes);

        var dependente = Assert.Single(dependentes);
        Assert.NotNull(dependente.PessoaFisicaDependenteId);
        Assert.Equal("Joao Silva", dependente.Nome);
        Assert.Equal("98765432100", dependente.Cpf);
        Assert.Equal("Filho", dependente.TipoParentesco);
        Assert.Equal("2018-05-10", dependente.DataNascimento);
        Assert.Equal("Maria Silva", dependente.NomeMae);
        Assert.Equal("Jose Silva", dependente.NomePai);

        var pessoaDependente = await _client.GetFromJsonAsync<PessoaFisicaTestResponse>($"/api/pessoas-fisicas/{dependente.PessoaFisicaDependenteId}");
        Assert.NotNull(pessoaDependente);
        Assert.Equal("Joao Silva", pessoaDependente.Nome);
        Assert.Equal("98765432100", pessoaDependente.Cpf);
        Assert.Equal("2018-05-10", pessoaDependente.DataNascimento);
        Assert.Equal("Maria Silva", pessoaDependente.NomeMae);
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
            faixaEtaria = "34-38",
            dataNascimento = "1990-01-01",
            nomeMae = "Denise de Moraes Rosa Mendes",
            nomePai = "Manoel Messias Mendes"
        });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PessoaFisicaTestResponse>()
            ?? throw new InvalidOperationException("Pessoa fisica response was empty.");
    }

    private async Task CreateClienteResponse(Guid leadId, Guid pessoaFisicaId)
    {
        var response = await _client.PostAsJsonAsync("/api/clientes", new
        {
            leadId,
            pessoaFisicaId
        });

        response.EnsureSuccessStatusCode();
    }

    private sealed record LeadTestResponse(Guid Id, string Nome, string Telefone, int QuantidadeVidas, string? Operadora, string? Email, string? DataEnvio, string? DataRetorno, string? DataAprovacao, string? TokenConsultaAnalise, string? RetornoAnalise, string? DataHoraEnvioAnalise, string WorkflowEtapa);
    private sealed record PessoaFisicaTestResponse(Guid Id, string Nome, string? Cpf, string? Email, string? Telefone, string? FaixaEtaria, string? DataNascimento, string? NomeMae, string? NomePai);
    private sealed record LeadAnaliseTestResponse(Guid LeadId, string? TokenConsulta, string? RetornoAnalise, string? DataHoraEnvioAnalise, string WorkflowEtapa);
    private sealed record DocumentoTestResponse(Guid Id, Guid LeadId, Guid DocumentoExternoId, string Tipo, string Papel, string? TipoParentesco, string? Cpf, string? CpfDependente, string? Cnpj, bool ExtracaoProcessada, bool Aprovado, string DataUpload, string? DataAprovacao, string? MotivoReprovacao);
    private sealed record DocumentoAprovacaoTestResponse(Guid Id, Guid DocumentoExternoId, bool Aprovado, string? DataAprovacao);
    private sealed record DependenteTestResponse(Guid Id, Guid PessoaFisicaId, Guid? PessoaFisicaDependenteId, string? Nome, string? Cpf, string? TipoParentesco, string? Email, string? Telefone, string? FaixaEtaria, string? DataNascimento, string? NomeMae, string? NomePai);
    private sealed record HistoricoTestResponse(Guid Id, Guid LeadId, string Tipo, string Data);
    private sealed record ScriptTestResponse(Guid Id, string Etapa, string Tipo, string Mensagem);
}
