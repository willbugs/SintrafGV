using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Interfaces;

public interface IEleicaoRepository
{
    Task<Eleicao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Eleicao?> ObterPorIdComPerguntasAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Eleicao>> ListarAsync(int skip, int take, StatusEleicao? status = null, CancellationToken cancellationToken = default);
    Task<int> ContarAsync(StatusEleicao? status = null, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> ContarVotosPorEleicaoAsync(IEnumerable<Guid> eleicaoIds, CancellationToken cancellationToken = default);
    Task<Eleicao> IncluirAsync(Eleicao eleicao, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Eleicao eleicao, CancellationToken cancellationToken = default);
}
