using System.ComponentModel.DataAnnotations;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
public sealed class SimulacoesController(CorretorDbContext db) : ControllerBase
{
    [HttpGet("api/leads/{leadId:guid}/simulacoes")]
    public async Task<ActionResult<IEnumerable<SimulacaoResponse>>> GetByLead(Guid leadId, CancellationToken cancellationToken)
    {
        var simulacoes = await db.Simulacoes.AsNoTracking()
            .Where(x => x.LeadId == leadId)
            .Select(x => new SimulacaoResponse(x.Id, x.LeadId, x.Link, x.Aprovada, x.DataEnvio))
            .ToListAsync(cancellationToken);

        return Ok(simulacoes);
    }

    [HttpGet("api/simulacoes/{id:guid}")]
    public async Task<ActionResult<SimulacaoResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var simulacao = await db.Simulacoes.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SimulacaoResponse(x.Id, x.LeadId, x.Link, x.Aprovada, x.DataEnvio))
            .FirstOrDefaultAsync(cancellationToken);

        return simulacao is null ? NotFound() : Ok(simulacao);
    }

    [HttpPost("api/leads/{leadId:guid}/simulacoes")]
    public async Task<ActionResult<SimulacaoResponse>> Post(Guid leadId, SimulacaoRequest request, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == leadId, cancellationToken);
        if (lead is null)
        {
            return NotFound();
        }

        if (request.Aprovada)
        {
            await DesaprovarOutrasSimulacoes(leadId, null, cancellationToken);
        }

        var simulacao = new Simulacao
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            Link = request.Link,
            Aprovada = request.Aprovada,
            DataEnvio = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
        };

        db.Simulacoes.Add(simulacao);
        lead.WorkflowEtapa = LeadWorkflowEtapa.Simulacao;
        db.Historicos.Add(new Historico
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            Tipo = request.Aprovada ? HistoricoTipo.AprovacaoSimulacao : HistoricoTipo.EnvioSimulacao,
            Data = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
        });
        await db.SaveChangesAsync(cancellationToken);

        var response = new SimulacaoResponse(simulacao.Id, simulacao.LeadId, simulacao.Link, simulacao.Aprovada, simulacao.DataEnvio);
        return CreatedAtAction(nameof(GetById), new { id = simulacao.Id }, response);
    }

    [HttpPut("api/simulacoes/{id:guid}")]
    public async Task<IActionResult> Put(Guid id, SimulacaoRequest request, CancellationToken cancellationToken)
    {
        var simulacao = await db.Simulacoes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (simulacao is null)
        {
            return NotFound();
        }

        simulacao.Link = request.Link;
        simulacao.Aprovada = request.Aprovada;

        if (request.Aprovada)
        {
            await DesaprovarOutrasSimulacoes(simulacao.LeadId, simulacao.Id, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/simulacoes/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var simulacao = await db.Simulacoes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (simulacao is null)
        {
            return NotFound();
        }

        db.Simulacoes.Remove(simulacao);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task DesaprovarOutrasSimulacoes(Guid leadId, Guid? simulacaoId, CancellationToken cancellationToken)
    {
        var outrasAprovadas = await db.Simulacoes
            .Where(x => x.LeadId == leadId && x.Aprovada && (!simulacaoId.HasValue || x.Id != simulacaoId.Value))
            .ToListAsync(cancellationToken);

        foreach (var outra in outrasAprovadas)
        {
            outra.Aprovada = false;
        }
    }
}

public sealed record SimulacaoRequest(string? Link, bool Aprovada);
public sealed record SimulacaoResponse(Guid Id, Guid LeadId, string? Link, bool Aprovada, string DataEnvio);
