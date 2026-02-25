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

    public async Task<int> ContarAsync(CancellationToken cancellationToken = default) =>
        await _context.Usuarios.CountAsync(cancellationToken);

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
