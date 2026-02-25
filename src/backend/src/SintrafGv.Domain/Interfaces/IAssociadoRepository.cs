using SintrafGv.Domain.Entities;

namespace SintrafGv.Domain.Interfaces;

public interface IAssociadoRepository
{
    Task<Associado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Associado?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Associado>> ListarAsync(int skip, int take, bool apenasAtivos = false, CancellationToken cancellationToken = default);
    Task<int> ContarAsync(bool apenasAtivos = false, CancellationToken cancellationToken = default);
    Task<int> ContarAssociadosAtivosAsync(CancellationToken cancellationToken = default);
    Task<List<Associado>> ObterPorIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
    Task<Associado> IncluirAsync(Associado associado, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Associado associado, CancellationToken cancellationToken = default);
}
