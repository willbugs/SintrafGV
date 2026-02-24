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
}
