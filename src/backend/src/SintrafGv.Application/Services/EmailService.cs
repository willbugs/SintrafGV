using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using SintrafGv.Application.Interfaces;
using SintrafGv.Domain.Interfaces;

namespace SintrafGv.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguracaoEmailRepository _configRepository;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguracaoEmailRepository configRepository, ILogger<EmailService> logger)
    {
        _configRepository = configRepository;
        _logger = logger;
    }

    public async Task<bool> EnviarEmailNovoUsuarioAsync(string destinatario, string nome, string senhaTemporaria, CancellationToken cancellationToken = default)
    {
        var config = await _configRepository.ObterAsync(cancellationToken);
        if (config == null || !config.Habilitado || string.IsNullOrWhiteSpace(config.SmtpHost))
        {
            _logger.LogWarning("Envio de e-mail desabilitado ou configuração inexistente.");
            return false;
        }

        var assunto = "Acesso ao Sistema SintrafGV";
        var corpo = $@"
Olá, {nome}!

Seu cadastro no painel administrativo do SintrafGV foi realizado.

Credenciais de acesso:
- E-mail: {destinatario}
- Senha temporária: {senhaTemporaria}

Recomendamos que você altere sua senha no primeiro acesso.

Acesse: https://admin.sintrafgv.com.br

—
Sistema SintrafGV
";

        return await EnviarAsync(config, destinatario, assunto, corpo, cancellationToken);
    }

    public async Task<bool> TestarConfiguracaoAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configRepository.ObterAsync(cancellationToken);
        if (config == null || !config.Habilitado || string.IsNullOrWhiteSpace(config.SmtpHost))
            return false;

        var assunto = "Teste de configuração - SintrafGV";
        var corpo = "Este é um e-mail de teste. A configuração de envio está funcionando corretamente.";
        return await EnviarAsync(config, config.EmailRemetente, assunto, corpo, cancellationToken);
    }

    private async Task<bool> EnviarAsync(Domain.Entities.ConfiguracaoEmail config, string destinatario, string assunto, string corpo, CancellationToken cancellationToken)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(config.NomeRemetente ?? "SintrafGV", config.EmailRemetente));
            message.To.Add(MailboxAddress.Parse(destinatario));
            message.Subject = assunto;
            message.Body = new TextPart("plain") { Text = corpo.Trim() };

            using var client = new SmtpClient();
            var secureSocketOptions = config.UsarSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(config.SmtpHost, config.SmtpPort, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrWhiteSpace(config.SmtpUsuario))
                await client.AuthenticateAsync(config.SmtpUsuario, config.SmtpSenha ?? "", cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("E-mail enviado com sucesso para {Destinatario}", destinatario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail para {Destinatario}", destinatario);
            return false;
        }
    }
}
