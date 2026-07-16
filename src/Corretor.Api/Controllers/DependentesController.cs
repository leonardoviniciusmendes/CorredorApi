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
            .ToListAsync(cancellationToken);

        return Ok(await MontarResponses(dependentes, cancellationToken));
    }

    [HttpPost("api/pessoas-fisicas/{pessoaFisicaId:guid}/dependentes")]
    public async Task<ActionResult<DependenteResponse>> Post(Guid pessoaFisicaId, DependenteRequest request, CancellationToken cancellationToken)
    {
        if (!await db.PessoasFisicas.AsNoTracking().AnyAsync(x => x.Id == pessoaFisicaId, cancellationToken))
        {
            return NotFound();
        }

        var pessoaDependente = new PessoaFisica
        {
            Id = Guid.NewGuid(),
            Nome = string.IsNullOrWhiteSpace(request.Nome) ? "Dependente" : request.Nome,
            Cpf = request.Cpf,
            Email = request.Email,
            Telefone = request.Telefone,
            FaixaEtaria = request.FaixaEtaria,
            DataNascimento = request.DataNascimento,
            NomeMae = request.NomeMae,
            NomePai = request.NomePai
        };

        var dependente = new Dependente
        {
            Id = Guid.NewGuid(),
            PessoaFisicaId = pessoaFisicaId,
            PessoaFisicaDependenteId = pessoaDependente.Id,
            Cpf = request.Cpf,
            TipoParentesco = request.TipoParentesco,
            DataNascimento = request.DataNascimento,
            NomeMae = request.NomeMae,
            NomePai = request.NomePai
        };
        db.PessoasFisicas.Add(pessoaDependente);
        db.Dependentes.Add(dependente);
        await db.SaveChangesAsync(cancellationToken);

        var response = (await MontarResponses([dependente], cancellationToken))[0];
        return Created($"/api/pessoas-fisicas/{pessoaFisicaId}/dependentes", response);
    }

    [HttpPut("api/dependentes/{id:guid}")]
    public async Task<IActionResult> Put(Guid id, DependenteRequest request, CancellationToken cancellationToken)
    {
        var dependente = await db.Dependentes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (dependente is null)
        {
            return NotFound();
        }

        dependente.Cpf = request.Cpf;
        dependente.TipoParentesco = request.TipoParentesco;
        dependente.DataNascimento = request.DataNascimento;
        dependente.NomeMae = request.NomeMae;
        dependente.NomePai = request.NomePai;

        PessoaFisica? pessoaDependente = null;
        if (dependente.PessoaFisicaDependenteId.HasValue)
        {
            pessoaDependente = await db.PessoasFisicas.FirstOrDefaultAsync(x => x.Id == dependente.PessoaFisicaDependenteId.Value, cancellationToken);
        }

        if (pessoaDependente is null)
        {
            pessoaDependente = new PessoaFisica
            {
                Id = Guid.NewGuid(),
                Nome = string.IsNullOrWhiteSpace(request.Nome) ? "Dependente" : request.Nome
            };
            dependente.PessoaFisicaDependenteId = pessoaDependente.Id;
            db.PessoasFisicas.Add(pessoaDependente);
        }

        pessoaDependente.Nome = string.IsNullOrWhiteSpace(request.Nome) ? pessoaDependente.Nome : request.Nome;
        pessoaDependente.Cpf = request.Cpf;
        pessoaDependente.Email = request.Email;
        pessoaDependente.Telefone = request.Telefone;
        pessoaDependente.FaixaEtaria = request.FaixaEtaria;
        pessoaDependente.DataNascimento = request.DataNascimento;
        pessoaDependente.NomeMae = request.NomeMae;
        pessoaDependente.NomePai = request.NomePai;

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

    private async Task<List<DependenteResponse>> MontarResponses(List<Dependente> dependentes, CancellationToken cancellationToken)
    {
        var pessoaDependenteIds = dependentes
            .Where(x => x.PessoaFisicaDependenteId.HasValue)
            .Select(x => x.PessoaFisicaDependenteId!.Value)
            .Distinct()
            .ToList();

        var pessoasDependentes = await db.PessoasFisicas.AsNoTracking()
            .Where(x => pessoaDependenteIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return dependentes
            .Select(dependente =>
            {
                pessoasDependentes.TryGetValue(dependente.PessoaFisicaDependenteId ?? Guid.Empty, out var pessoa);

                return new DependenteResponse(
                    dependente.Id,
                    dependente.PessoaFisicaId,
                    dependente.PessoaFisicaDependenteId,
                    pessoa?.Nome,
                    pessoa?.Cpf ?? dependente.Cpf,
                    dependente.TipoParentesco,
                    pessoa?.Email,
                    pessoa?.Telefone,
                    pessoa?.FaixaEtaria,
                    pessoa?.DataNascimento ?? dependente.DataNascimento,
                    pessoa?.NomeMae ?? dependente.NomeMae,
                    pessoa?.NomePai ?? dependente.NomePai);
            })
            .ToList();
    }
}

public sealed record DependenteRequest(string? Nome, string? Cpf, string? TipoParentesco, string? Email, string? Telefone, string? FaixaEtaria, string? DataNascimento, string? NomeMae, string? NomePai);
public sealed record DependenteResponse(Guid Id, Guid PessoaFisicaId, Guid? PessoaFisicaDependenteId, string? Nome, string? Cpf, string? TipoParentesco, string? Email, string? Telefone, string? FaixaEtaria, string? DataNascimento, string? NomeMae, string? NomePai);
