using Microsoft.EntityFrameworkCore;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Domain.Entities;
using SintrafGv.Infrastructure.Data;

namespace SintrafGv.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context) => _context = context;

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Usuarios.FindAsync([id], cancellationToken);

    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<Usuario?> ObterPorEmailExcluindoIdAsync(string email, Guid excluirId, CancellationToken cancellationToken = default) =>
        await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email && u.Id != excluirId, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(int skip, int take, CancellationToken cancellationToken = default) =>
        await _context.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nome)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(int skip, int take, string? busca, string? role, bool? ativo, CancellationToken cancellationToken = default)
    {
        var query = AplicarFiltros(_context.Usuarios.AsNoTracking(), busca, role, ativo);
        return await query.OrderBy(u => u.Nome).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(CancellationToken cancellationToken = default) =>
        await _context.Usuarios.CountAsync(cancellationToken);

    public async Task<int> ContarAsync(string? busca, string? role, bool? ativo, CancellationToken cancellationToken = default)
    {
        var query = AplicarFiltros(_context.Usuarios.AsNoTracking(), busca, role, ativo);
        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<Usuario> AplicarFiltros(IQueryable<Usuario> query, string? busca, string? role, bool? ativo)
    {
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(u =>
                (u.Nome != null && u.Nome.Contains(termo)) ||
                (u.Email != null && u.Email.Contains(termo)));
        }
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);
        if (ativo.HasValue)
            query = query.Where(u => u.Ativo == ativo.Value);
        return query;
    }

    public async Task<Usuario> IncluirAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(cancellationToken);
        return usuario;
    }

    public async Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
