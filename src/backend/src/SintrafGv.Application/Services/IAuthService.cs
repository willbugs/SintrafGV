using SintrafGv.Application.DTOs;

namespace SintrafGv.Application.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<LoginResponse?> RefreshAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> AlterarSenhaAsync(Guid userId, string senhaAtual, string novaSenha, CancellationToken cancellationToken = default);
}
