namespace SintrafGv.Application.Interfaces;

public interface IEmailService
{
    Task<bool> EnviarEmailNovoUsuarioAsync(string destinatario, string nome, string senhaTemporaria, CancellationToken cancellationToken = default);
    Task<bool> TestarConfiguracaoAsync(CancellationToken cancellationToken = default);
}
