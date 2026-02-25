using Microsoft.EntityFrameworkCore;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Domain.Entities;
using SintrafGv.Infrastructure.Data;

namespace SintrafGv.Infrastructure.Repositories;

public class AssociadoRepository : IAssociadoRepository
{
    private readonly AppDbContext _context;

    public AssociadoRepository(AppDbContext context) => _context = context;

    public async Task<Associado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Associados.FindAsync([id], cancellationToken);

    public async Task<Associado?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default) =>
        await _context.Associados.FirstOrDefaultAsync(a => a.Cpf == cpf, cancellationToken);

    public async Task<IReadOnlyList<Associado>> ListarAsync(int skip, int take, bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Associados.AsNoTracking();
        if (apenasAtivos)
            query = query.Where(x => x.Ativo);
        return await query.OrderBy(x => x.Nome).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Associados.AsNoTracking();
        if (apenasAtivos)
            query = query.Where(x => x.Ativo);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<Associado> IncluirAsync(Associado associado, CancellationToken cancellationToken = default)
    {
        _context.Associados.Add(associado);
        await _context.SaveChangesAsync(cancellationToken);
        return associado;
    }

    public async Task<int> ContarAssociadosAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Associados
            .CountAsync(a => a.Ativo, cancellationToken);
    }

    public async Task<List<Associado>> ObterPorIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Associados
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Associado associado, CancellationToken cancellationToken = default)
    {
        _context.Associados.Update(associado);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
