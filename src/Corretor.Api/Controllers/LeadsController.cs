using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
[Route("api/leads")]
public sealed class LeadsController(CorretorDbContext db, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeadResponse>>> Get(CancellationToken cancellationToken)
    {
        var leads = await db.Leads.AsNoTracking()
            .Select(x => new LeadResponse(
                x.Id,
                x.Nome,
                x.Telefone,
                x.QuantidadeVidas,
                x.Operadora,
                x.Email,
                x.DataEnvio,
                x.DataRetorno,
                x.DataAprovacao,
                x.TokenConsultaAnalise,
                x.RetornoAnalise,
                x.DataHoraEnvioAnalise,
                x.WorkflowEtapa))
            .ToListAsync(cancellationToken);

        return Ok(leads);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeadResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LeadResponse(
                x.Id,
                x.Nome,
                x.Telefone,
                x.QuantidadeVidas,
                x.Operadora,
                x.Email,
                x.DataEnvio,
                x.DataRetorno,
                x.DataAprovacao,
                x.TokenConsultaAnalise,
                x.RetornoAnalise,
                x.DataHoraEnvioAnalise,
                x.WorkflowEtapa))
            .FirstOrDefaultAsync(cancellationToken);

        return lead is null ? NotFound() : Ok(lead);
    }

    [HttpGet("{id:guid}/ficha-associativa/pdf")]
    public async Task<IActionResult> GerarFichaAssociativa(Guid id, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new FichaLeadDados(x.Nome, x.Telefone, x.QuantidadeVidas, x.Operadora, x.Email, x.DataEnvio, x.DataRetorno, x.DataAprovacao))
            .FirstOrDefaultAsync(cancellationToken);

        if (lead is null)
        {
            return NotFound();
        }

        var cliente = await db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.LeadId == id, cancellationToken);

        FichaPessoaFisicaDados? pessoaFisica = null;
        FichaPessoaJuridicaDados? pessoaJuridica = null;
        List<FichaEnderecoDados> enderecos = [];
        List<FichaDependenteDados> dependentes = [];

        if (cliente is not null)
        {
            if (cliente.PessoaFisicaId.HasValue)
            {
                pessoaFisica = await db.PessoasFisicas.AsNoTracking()
                    .Where(x => x.Id == cliente.PessoaFisicaId.Value)
                    .Select(x => new FichaPessoaFisicaDados(x.Id, x.Nome, x.Cpf, x.Email, x.Telefone, x.FaixaEtaria, x.DataNascimento, x.NomeMae, x.NomePai))
                    .FirstOrDefaultAsync(cancellationToken);

                var dependentesEntidades = await db.Dependentes.AsNoTracking()
                    .Where(x => x.PessoaFisicaId == cliente.PessoaFisicaId.Value)
                    .ToListAsync(cancellationToken);

                var pessoaDependenteIds = dependentesEntidades
                    .Where(x => x.PessoaFisicaDependenteId.HasValue)
                    .Select(x => x.PessoaFisicaDependenteId!.Value)
                    .Distinct()
                    .ToList();

                var pessoasDependentes = await db.PessoasFisicas.AsNoTracking()
                    .Where(x => pessoaDependenteIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

                dependentes = dependentesEntidades
                    .Select(dependente =>
                    {
                        pessoasDependentes.TryGetValue(dependente.PessoaFisicaDependenteId ?? Guid.Empty, out var pessoaDependente);
                        return new FichaDependenteDados(
                            dependente.Id,
                            dependente.PessoaFisicaId,
                            dependente.PessoaFisicaDependenteId,
                            pessoaDependente?.Nome,
                            pessoaDependente?.Cpf ?? dependente.Cpf,
                            pessoaDependente?.DataNascimento ?? dependente.DataNascimento,
                            pessoaDependente?.NomeMae ?? dependente.NomeMae,
                            pessoaDependente?.NomePai ?? dependente.NomePai);
                    })
                    .ToList();
            }

            if (cliente.PessoaJuridicaId.HasValue)
            {
                pessoaJuridica = await db.PessoasJuridicas.AsNoTracking()
                    .Where(x => x.Id == cliente.PessoaJuridicaId.Value)
                    .Select(x => new FichaPessoaJuridicaDados(x.Id, x.NomeEmpresa, x.Cnpj, x.IE, x.Email, x.Telefone, x.DataAbertura))
                    .FirstOrDefaultAsync(cancellationToken);
            }

            enderecos = await db.Enderecos.AsNoTracking()
                .Where(x => x.ClienteId == cliente.Id)
                .Select(x => new FichaEnderecoDados(x.Logradouro, x.Estado, x.Cidade, x.Cep))
                .ToListAsync(cancellationToken);
        }

        var faixasEtarias = await db.FaixasEtarias.AsNoTracking()
            .Where(x => x.LeadId == id)
            .Select(x => new FichaFaixaEtariaDados(x.Faixa, x.Quantidade))
            .ToListAsync(cancellationToken);

        var dados = new FichaAssociativaDados(lead, pessoaFisica, pessoaJuridica, enderecos, faixasEtarias, dependentes);
        var templatePath = Path.Combine(environment.ContentRootPath, "Templates", "FichaAssociativaAnaspl.pdf");
        if (!System.IO.File.Exists(templatePath))
        {
            return Problem("Template da ficha associativa nao encontrado.");
        }

        var pdf = FichaAssociativaPdfGenerator.Gerar(dados, templatePath);
        var fileName = $"ficha-associativa-{id}.pdf";

        return File(pdf, "application/pdf", fileName);
    }

    [HttpPost]
    public async Task<ActionResult<LeadResponse>> Post(LeadRequest request, CancellationToken cancellationToken)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Telefone = request.Telefone,
            QuantidadeVidas = request.QuantidadeVidas,
            Operadora = request.Operadora,
            Email = request.Email,
            DataEnvio = request.DataEnvio,
            DataRetorno = request.DataRetorno,
            DataAprovacao = request.DataAprovacao,
            WorkflowEtapa = LeadWorkflowEtapa.Lead
        };

        db.Leads.Add(lead);
        db.Historicos.Add(new Historico
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            Tipo = HistoricoTipo.CadastroLead,
            Data = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
        });
        await db.SaveChangesAsync(cancellationToken);

        var response = ToResponse(lead);
        return CreatedAtAction(nameof(GetById), new { id = lead.Id }, response);
    }

    [HttpPost("{id:guid}/analise")]
    public async Task<ActionResult<LeadAnaliseResponse>> RegistrarAnalise(Guid id, LeadAnaliseRequest request, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lead is null)
        {
            return NotFound();
        }

        lead.TokenConsultaAnalise = request.TokenConsulta;
        lead.RetornoAnalise = GetRetornoAnalise(request.RetornoAnalise);
        lead.DataHoraEnvioAnalise = DateTimeOffset.UtcNow.ToString("O");
        lead.WorkflowEtapa = LeadWorkflowEtapa.Analise;

        db.Historicos.Add(new Historico
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            Tipo = HistoricoTipo.RetornoLead,
            Data = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
        });

        await db.SaveChangesAsync(cancellationToken);

        return Ok(new LeadAnaliseResponse(
            lead.Id,
            lead.TokenConsultaAnalise,
            lead.RetornoAnalise,
            lead.DataHoraEnvioAnalise,
            lead.WorkflowEtapa));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, LeadRequest request, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lead is null)
        {
            return NotFound();
        }

        lead.Nome = request.Nome;
        lead.Telefone = request.Telefone;
        lead.QuantidadeVidas = request.QuantidadeVidas;
        lead.Operadora = request.Operadora;
        lead.Email = request.Email;
        lead.DataEnvio = request.DataEnvio;
        lead.DataRetorno = request.DataRetorno;
        lead.DataAprovacao = request.DataAprovacao;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lead is null)
        {
            return NotFound();
        }

        db.Leads.Remove(lead);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static LeadResponse ToResponse(Lead lead)
    {
        return new LeadResponse(
            lead.Id,
            lead.Nome,
            lead.Telefone,
            lead.QuantidadeVidas,
            lead.Operadora,
            lead.Email,
            lead.DataEnvio,
            lead.DataRetorno,
            lead.DataAprovacao,
            lead.TokenConsultaAnalise,
            lead.RetornoAnalise,
            lead.DataHoraEnvioAnalise,
            lead.WorkflowEtapa);
    }

    private static string? GetRetornoAnalise(JsonElement retornoAnalise)
    {
        return retornoAnalise.ValueKind switch
        {
            JsonValueKind.Undefined or JsonValueKind.Null => null,
            JsonValueKind.String => retornoAnalise.GetString(),
            _ => retornoAnalise.GetRawText()
        };
    }
}

public sealed record LeadRequest([Required] string Nome, [Required] string Telefone, int QuantidadeVidas, string? Operadora, string? Email, string? DataEnvio, string? DataRetorno, string? DataAprovacao);
public sealed record LeadResponse(Guid Id, string Nome, string Telefone, int QuantidadeVidas, string? Operadora, string? Email, string? DataEnvio, string? DataRetorno, string? DataAprovacao, string? TokenConsultaAnalise, string? RetornoAnalise, string? DataHoraEnvioAnalise, LeadWorkflowEtapa WorkflowEtapa);
public sealed record LeadAnaliseRequest([Required] string TokenConsulta, JsonElement RetornoAnalise);
public sealed record LeadAnaliseResponse(Guid LeadId, string? TokenConsulta, string? RetornoAnalise, string? DataHoraEnvioAnalise, LeadWorkflowEtapa WorkflowEtapa);
