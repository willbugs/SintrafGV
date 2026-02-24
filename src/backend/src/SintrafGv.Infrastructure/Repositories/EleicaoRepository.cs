using Microsoft.EntityFrameworkCore;
using SintrafGv.Application.Interfaces;
using SintrafGv.Domain.Entities;
using SintrafGv.Infrastructure.Data;

namespace SintrafGv.Infrastructure.Repositories;

public class EleicaoRepository : IEleicaoRepository
{
    private readonly AppDbContext _context;

    public EleicaoRepository(AppDbContext context) => _context = context;

    public async Task<Eleicao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Eleicoes.FindAsync([id], cancellationToken);

    public async Task<Eleicao?> ObterPorIdComPerguntasAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Eleicoes
            .Include(e => e.Perguntas.OrderBy(p => p.Ordem))
            .ThenInclude(p => p.Opcoes.OrderBy(o => o.Ordem))
            .ThenInclude(o => o.Associado)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Eleicao>> ListarAsync(int skip, int take, StatusEleicao? status, CancellationToken cancellationToken = default)
    {
        var query = _context.Eleicoes.AsNoTracking();
        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);
        return await query
            .Include(e => e.Perguntas)
            .OrderByDescending(e => e.CriadoEm)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(StatusEleicao? status, CancellationToken cancellationToken = default)
    {
        var query = _context.Eleicoes.AsNoTracking();
        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> ContarVotosPorEleicaoAsync(IEnumerable<Guid> eleicaoIds, CancellationToken cancellationToken = default)
    {
        var ids = eleicaoIds.ToList();
        if (ids.Count == 0) return new Dictionary<Guid, int>();
        var contagens = await _context.Votos
            .Where(v => ids.Contains(v.EleicaoId))
            .GroupBy(v => v.EleicaoId)
            .Select(g => new { EleicaoId = g.Key, Total = g.Count() })
            .ToListAsync(cancellationToken);
        return contagens.ToDictionary(x => x.EleicaoId, x => x.Total);
    }

    public async Task<Eleicao> IncluirAsync(Eleicao eleicao, CancellationToken cancellationToken = default)
    {
        _context.Eleicoes.Add(eleicao);
        await _context.SaveChangesAsync(cancellationToken);
        return eleicao;
    }

    public async Task AtualizarAsync(Eleicao eleicao, CancellationToken cancellationToken = default)
    {
        eleicao.AtualizadoEm = DateTime.UtcNow;
        _context.Eleicoes.Update(eleicao);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // MÉTODOS PARA APURAÇÃO
    public async Task<int> ContarVotantesPorEleicaoAsync(Guid eleicaoId, CancellationToken cancellationToken = default) =>
        await _context.Votos.CountAsync(v => v.EleicaoId == eleicaoId, cancellationToken);

    public async Task<Dictionary<Guid, int>> ContarVotosPorOpcaoAsync(Guid eleicaoId, CancellationToken cancellationToken = default)
    {
        var votos = await _context.VotosDetalhes
            .Include(vd => vd.Pergunta)
            .Where(vd => vd.Pergunta!.EleicaoId == eleicaoId && vd.OpcaoId != null)
            .GroupBy(vd => vd.OpcaoId!.Value)
            .Select(g => new { OpcaoId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        return votos.ToDictionary(v => v.OpcaoId, v => v.Count);
    }

    public async Task<Dictionary<Guid, int>> ContarVotosBrancoPorPerguntaAsync(Guid eleicaoId, CancellationToken cancellationToken = default)
    {
        var votosBranco = await _context.VotosDetalhes
            .Include(vd => vd.Pergunta)
            .Where(vd => vd.Pergunta!.EleicaoId == eleicaoId && (vd.OpcaoId == null || vd.VotoBranco))
            .GroupBy(vd => vd.PerguntaId)
            .Select(g => new { PerguntaId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        return votosBranco.ToDictionary(v => v.PerguntaId, v => v.Count);
    }

    public async Task<bool> AssociadoJaVotouAsync(Guid eleicaoId, Guid associadoId, CancellationToken cancellationToken = default) =>
        await _context.Votos.AnyAsync(v => v.EleicaoId == eleicaoId && v.AssociadoId == associadoId, cancellationToken);

    public async Task<Voto> RegistrarVotoAsync(Voto voto, List<VotoDetalhe> detalhes, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _context.Votos.Add(voto);
            await _context.SaveChangesAsync(cancellationToken);

            _context.VotosDetalhes.AddRange(detalhes);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return voto;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
