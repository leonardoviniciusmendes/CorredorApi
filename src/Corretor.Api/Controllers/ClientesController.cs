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
    public async Task<ActionResult<IEnumerable<ClienteResponse>>> Get(CancellationToken cancellationToken)
    {
        var clientes = await db.Clientes.AsNoTracking()
            .Select(x => new ClienteResponse(x.Id, x.LeadId, x.PessoaFisicaId, x.PessoaJuridicaId))
            .ToListAsync(cancellationToken);

        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ClienteResponse(x.Id, x.LeadId, x.PessoaFisicaId, x.PessoaJuridicaId))
            .FirstOrDefaultAsync(cancellationToken);

        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Post(ClienteRequest request, CancellationToken cancellationToken)
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

        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, new ClienteResponse(cliente.Id, cliente.LeadId, cliente.PessoaFisicaId, cliente.PessoaJuridicaId));
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
}

public sealed record ClienteRequest([Required] Guid LeadId, Guid? PessoaFisicaId, Guid? PessoaJuridicaId);
public sealed record ClienteResponse(Guid Id, Guid LeadId, Guid? PessoaFisicaId, Guid? PessoaJuridicaId);
