using System.ComponentModel.DataAnnotations;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController(CorretorDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDetalhadoResponse>>> Get(CancellationToken cancellationToken)
    {
        var clientes = await db.Clientes.AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(await MontarResponses(clientes, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteDetalhadoResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (cliente is null)
        {
            return NotFound();
        }

        var response = await MontarResponses([cliente], cancellationToken);
        return Ok(response[0]);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDetalhadoResponse>> Post(ClienteRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Leads.AsNoTracking().AnyAsync(x => x.Id == request.LeadId, cancellationToken))
        {
            return NotFound();
        }

        var pessoaFisicaInformada = request.PessoaFisicaId.HasValue;
        var pessoaJuridicaInformada = request.PessoaJuridicaId.HasValue;
        if (pessoaFisicaInformada == pessoaJuridicaInformada)
        {
            return BadRequest();
        }

        if (request.PessoaFisicaId.HasValue &&
            !await db.PessoasFisicas.AsNoTracking().AnyAsync(x => x.Id == request.PessoaFisicaId.Value, cancellationToken))
        {
            return NotFound();
        }

        if (request.PessoaJuridicaId.HasValue &&
            !await db.PessoasJuridicas.AsNoTracking().AnyAsync(x => x.Id == request.PessoaJuridicaId.Value, cancellationToken))
        {
            return NotFound();
        }

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            LeadId = request.LeadId,
            PessoaFisicaId = request.PessoaFisicaId,
            PessoaJuridicaId = request.PessoaJuridicaId
        };

        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(cancellationToken);

        var response = await MontarResponses([cliente], cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, response[0]);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cliente is null)
        {
            return NotFound();
        }

        db.Clientes.Remove(cliente);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<List<ClienteDetalhadoResponse>> MontarResponses(List<Cliente> clientes, CancellationToken cancellationToken)
    {
        var leadIds = clientes.Select(x => x.LeadId).Distinct().ToList();
        var pessoaFisicaIds = clientes.Where(x => x.PessoaFisicaId.HasValue).Select(x => x.PessoaFisicaId!.Value).Distinct().ToList();
        var pessoaJuridicaIds = clientes.Where(x => x.PessoaJuridicaId.HasValue).Select(x => x.PessoaJuridicaId!.Value).Distinct().ToList();
        var clienteIds = clientes.Select(x => x.Id).Distinct().ToList();

        var leads = await db.Leads.AsNoTracking()
            .Where(x => leadIds.Contains(x.Id))
            .Select(x => new ClienteLeadResponse(
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
                x.WorkflowEtapa.ToString()))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var pessoasFisicas = await db.PessoasFisicas.AsNoTracking()
            .Where(x => pessoaFisicaIds.Contains(x.Id))
            .Select(x => new ClientePessoaFisicaResponse(x.Id, x.Nome, x.Cpf, x.Email, x.Telefone, x.FaixaEtaria, x.DataNascimento, x.NomeMae, x.NomePai, new List<ClienteDependenteResponse>()))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var pessoasJuridicas = await db.PessoasJuridicas.AsNoTracking()
            .Where(x => pessoaJuridicaIds.Contains(x.Id))
            .Select(x => new ClientePessoaJuridicaResponse(x.Id, x.NomeEmpresa, x.Cnpj, x.IE, x.Email, x.Telefone, x.DataAbertura))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var dependentes = await db.Dependentes.AsNoTracking()
            .Where(x => pessoaFisicaIds.Contains(x.PessoaFisicaId))
            .ToListAsync(cancellationToken);

        var pessoaFisicaDependenteIds = dependentes
            .Where(x => x.PessoaFisicaDependenteId.HasValue)
            .Select(x => x.PessoaFisicaDependenteId!.Value)
            .Distinct()
            .ToList();

        var pessoasFisicasDependentes = await db.PessoasFisicas.AsNoTracking()
            .Where(x => pessoaFisicaDependenteIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var dependentesResponses = dependentes
            .Select(dependente =>
            {
                pessoasFisicasDependentes.TryGetValue(dependente.PessoaFisicaDependenteId ?? Guid.Empty, out var pessoaDependente);

                return new ClienteDependenteResponse(
                    dependente.Id,
                    dependente.PessoaFisicaId,
                    dependente.PessoaFisicaDependenteId,
                    pessoaDependente?.Nome,
                    pessoaDependente?.Cpf ?? dependente.Cpf,
                    dependente.TipoParentesco,
                    pessoaDependente?.Email,
                    pessoaDependente?.Telefone,
                    pessoaDependente?.FaixaEtaria,
                    pessoaDependente?.DataNascimento ?? dependente.DataNascimento,
                    pessoaDependente?.NomeMae ?? dependente.NomeMae,
                    pessoaDependente?.NomePai ?? dependente.NomePai);
            })
            .ToList();

        var dependentesPorPessoaFisica = dependentesResponses
            .GroupBy(x => x.PessoaFisicaId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var enderecos = await db.Enderecos.AsNoTracking()
            .Where(x => clienteIds.Contains(x.ClienteId))
            .Select(x => new ClienteEnderecoResponse(x.Id, x.ClienteId, x.Logradouro, x.Estado, x.Cidade, x.Cep))
            .ToListAsync(cancellationToken);

        var enderecosPorCliente = enderecos
            .GroupBy(x => x.ClienteId)
            .ToDictionary(x => x.Key, x => x.ToList());

        return clientes
            .Select(cliente =>
            {
                ClientePessoaFisicaResponse? pessoaFisica = null;
                if (cliente.PessoaFisicaId.HasValue && pessoasFisicas.TryGetValue(cliente.PessoaFisicaId.Value, out var pf))
                {
                    dependentesPorPessoaFisica.TryGetValue(pf.Id, out var dependentes);
                    pessoaFisica = pf with { Dependentes = dependentes ?? [] };
                }

                pessoasJuridicas.TryGetValue(cliente.PessoaJuridicaId ?? Guid.Empty, out var pessoaJuridica);
                leads.TryGetValue(cliente.LeadId, out var lead);
                enderecosPorCliente.TryGetValue(cliente.Id, out var enderecos);

                return new ClienteDetalhadoResponse(
                    cliente.Id,
                    cliente.LeadId,
                    cliente.PessoaFisicaId,
                    cliente.PessoaJuridicaId,
                    lead,
                    pessoaFisica,
                    pessoaJuridica,
                    enderecos ?? []);
            })
            .ToList();
    }
}

public sealed record ClienteRequest([Required] Guid LeadId, Guid? PessoaFisicaId, Guid? PessoaJuridicaId);
public sealed record ClienteDetalhadoResponse(
    Guid Id,
    Guid LeadId,
    Guid? PessoaFisicaId,
    Guid? PessoaJuridicaId,
    ClienteLeadResponse? Lead,
    ClientePessoaFisicaResponse? PessoaFisica,
    ClientePessoaJuridicaResponse? PessoaJuridica,
    List<ClienteEnderecoResponse> Enderecos);

public sealed record ClienteLeadResponse(Guid Id, string Nome, string Telefone, int QuantidadeVidas, string? Operadora, string? Email, string? DataEnvio, string? DataRetorno, string? DataAprovacao, string? TokenConsultaAnalise, string? RetornoAnalise, string? DataHoraEnvioAnalise, string WorkflowEtapa);
public sealed record ClientePessoaFisicaResponse(Guid Id, string Nome, string? Cpf, string? Email, string? Telefone, string? FaixaEtaria, string? DataNascimento, string? NomeMae, string? NomePai, List<ClienteDependenteResponse> Dependentes);
public sealed record ClientePessoaJuridicaResponse(Guid Id, string NomeEmpresa, string Cnpj, string IE, string? Email, string? Telefone, DateTime DataAbertura);
public sealed record ClienteEnderecoResponse(Guid Id, Guid ClienteId, string Logradouro, string Estado, string Cidade, string Cep);
public sealed record ClienteDependenteResponse(Guid Id, Guid PessoaFisicaId, Guid? PessoaFisicaDependenteId, string? Nome, string? Cpf, string? TipoParentesco, string? Email, string? Telefone, string? FaixaEtaria, string? DataNascimento, string? NomeMae, string? NomePai);
