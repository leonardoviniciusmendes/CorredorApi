using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
public sealed class DependentesController(CorretorDbContext db) : ControllerBase
{
    [HttpGet("api/pessoas-fisicas/{pessoaFisicaId:guid}/dependentes")]
    public async Task<ActionResult<IEnumerable<DependenteResponse>>> GetByPessoaFisica(Guid pessoaFisicaId, CancellationToken cancellationToken)
    {
        var dependentes = await db.Dependentes.AsNoTracking()
            .Where(x => x.PessoaFisicaId == pessoaFisicaId)
            .Select(x => new DependenteResponse(x.Id, x.PessoaFisicaId, x.DataNascimento, x.NomeMae, x.NomePai))
            .ToListAsync(cancellationToken);

        return Ok(dependentes);
    }

    [HttpPost("api/pessoas-fisicas/{pessoaFisicaId:guid}/dependentes")]
    public async Task<ActionResult<DependenteResponse>> Post(Guid pessoaFisicaId, DependenteRequest request, CancellationToken cancellationToken)
    {
        if (!await db.PessoasFisicas.AsNoTracking().AnyAsync(x => x.Id == pessoaFisicaId, cancellationToken))
        {
            return NotFound();
        }

        var dependente = new Dependente
        {
            Id = Guid.NewGuid(),
            PessoaFisicaId = pessoaFisicaId,
            DataNascimento = request.DataNascimento,
            NomeMae = request.NomeMae,
            NomePai = request.NomePai
        };
        db.Dependentes.Add(dependente);
        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/pessoas-fisicas/{pessoaFisicaId}/dependentes", ToResponse(dependente));
    }

    [HttpPut("api/dependentes/{id:guid}")]
    public async Task<IActionResult> Put(Guid id, DependenteRequest request, CancellationToken cancellationToken)
    {
        var dependente = await db.Dependentes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (dependente is null)
        {
            return NotFound();
        }

        dependente.DataNascimento = request.DataNascimento;
        dependente.NomeMae = request.NomeMae;
        dependente.NomePai = request.NomePai;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/dependentes/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var dependente = await db.Dependentes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (dependente is null)
        {
            return NotFound();
        }

        db.Dependentes.Remove(dependente);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static DependenteResponse ToResponse(Dependente dependente)
    {
        return new DependenteResponse(dependente.Id, dependente.PessoaFisicaId, dependente.DataNascimento, dependente.NomeMae, dependente.NomePai);
    }
}

public sealed record DependenteRequest(string? DataNascimento, string? NomeMae, string? NomePai);
public sealed record DependenteResponse(Guid Id, Guid PessoaFisicaId, string? DataNascimento, string? NomeMae, string? NomePai);
