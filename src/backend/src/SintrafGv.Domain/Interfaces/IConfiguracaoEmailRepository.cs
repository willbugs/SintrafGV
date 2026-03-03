using SintrafGv.Domain.Entities;

namespace SintrafGv.Domain.Interfaces;

public interface IConfiguracaoEmailRepository
{
    Task<ConfiguracaoEmail?> ObterAsync(CancellationToken cancellationToken = default);
    Task<ConfiguracaoEmail> SalvarAsync(ConfiguracaoEmail config, CancellationToken cancellationToken = default);
}
