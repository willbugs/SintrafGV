using SintrafGv.Domain.Entities;

namespace SintrafGv.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailExcluindoIdAsync(string email, Guid excluirId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Usuario>> ListarAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> ContarAsync(CancellationToken cancellationToken = default);
    Task<Usuario> IncluirAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
