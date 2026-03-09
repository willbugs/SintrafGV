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

    public async Task<Associado?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfDigits = new string(cpf.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(cpfDigits)) return null;
        // Tenta match exato primeiro
        var associado = await _context.Associados.FirstOrDefaultAsync(a => a.Cpf == cpfDigits, cancellationToken);
        if (associado != null) return associado;
        // Fallback: CPF no banco pode estar formatado (942.262.026-00)
        var associados = await _context.Associados
            .FromSqlRaw(
                "SELECT * FROM Associados WHERE REPLACE(REPLACE(REPLACE(ISNULL(Cpf,''), '.', ''), '-', ''), ' ', '') = {0}",
                cpfDigits)
            .ToListAsync(cancellationToken);
        return associados.FirstOrDefault();
    }

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

    public async Task<List<string>> ObterCidadesDistintasAsync(CancellationToken cancellationToken = default)
    {
        var raw = await _context.Associados
            .AsNoTracking()
            .Select(a => a.Cidade)
            .Distinct()
            .ToListAsync(cancellationToken);

        var cidades = raw
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (raw.Any(c => string.IsNullOrWhiteSpace(c)))
            cidades.Add("Não informado");

        return cidades.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<List<string>> ObterBancosDistintosAsync(CancellationToken cancellationToken = default)
    {
        var raw = await _context.Associados
            .AsNoTracking()
            .Select(a => a.Banco)
            .Distinct()
            .ToListAsync(cancellationToken);

        return raw
            .Select(b => string.IsNullOrWhiteSpace(b) ? "Não informado" : b.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
