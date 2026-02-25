using SintrafGv.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SintrafGv.Domain.Interfaces
{
    public interface IConfiguracaoSindicatoRepository
    {
        Task<ConfiguracaoSindicato?> ObterConfiguracaoAsync(CancellationToken cancellationToken = default);
        Task<ConfiguracaoSindicato> SalvarConfiguracaoAsync(ConfiguracaoSindicato configuracao, CancellationToken cancellationToken = default);
        Task<bool> ExisteConfiguracaoAsync(CancellationToken cancellationToken = default);
    }
}