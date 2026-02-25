using SintrafGv.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SintrafGv.Domain.Interfaces
{
    public interface IVotoRepository
    {
        Task<List<Voto>> ObterVotosPorEleicaoAsync(Guid eleicaoId, CancellationToken cancellationToken = default);
        Task<Voto?> ObterVotoPorIdAsync(Guid votoId, CancellationToken cancellationToken = default);
        Task<bool> VerificarVotoExistenteAsync(Guid eleicaoId, Guid associadoId, CancellationToken cancellationToken = default);
        Task<Voto> SalvarVotoAsync(Voto voto, CancellationToken cancellationToken = default);
        Task<int> ContarVotosPorEleicaoAsync(Guid eleicaoId, CancellationToken cancellationToken = default);
        Task<List<Voto>> ObterVotosPorAssociadoAsync(Guid associadoId, CancellationToken cancellationToken = default);
        Task<List<Voto>> ListarTodosAsync(CancellationToken cancellationToken = default);
        Task<List<Voto>> ListarPorEleicaoAsync(Guid eleicaoId, CancellationToken cancellationToken = default);
    }
}