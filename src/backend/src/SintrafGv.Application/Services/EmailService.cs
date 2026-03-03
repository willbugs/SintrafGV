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

        var (sucesso, _) = await EnviarAsync(config, destinatario, assunto, corpo, cancellationToken);
        return sucesso;
    }

    public async Task<(bool Sucesso, string? Erro)> TestarConfiguracaoAsync(string destinatario, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(destinatario))
            return (false, "E-mail de destino não informado.");

        var config = await _configRepository.ObterAsync(cancellationToken);
        if (config == null || !config.Habilitado || string.IsNullOrWhiteSpace(config.SmtpHost))
            return (false, "Configuração de e-mail não habilitada ou incompleta. Salve e habilite.");

        var assunto = "Teste de configuração - SintrafGV";
        var corpo = "Este é um e-mail de teste. A configuração de envio está funcionando corretamente.";
        return await EnviarAsync(config, destinatario.Trim(), assunto, corpo, cancellationToken);
    }

    private async Task<(bool Sucesso, string? Erro)> EnviarAsync(Domain.Entities.ConfiguracaoEmail config, string destinatario, string assunto, string corpo, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(config.NomeRemetente ?? "SintrafGV", config.EmailRemetente));
        message.To.Add(MailboxAddress.Parse(destinatario));
        message.Subject = assunto;
        message.Body = new TextPart("plain") { Text = corpo.Trim() };

        var usuariosParaTestar = ObterVariacoesUsuario(config.SmtpUsuario);
        var opcoesSsl = ObterOpcoesSslParaTestar(config);

        Exception? ultimoErro = null;
        foreach (var secureOpt in opcoesSsl)
        {
            foreach (var usuario in usuariosParaTestar)
            {
                try
                {
                    using var client = new SmtpClient();
                    client.ServerCertificateValidationCallback = (_, _, _, _) => true;
                    await client.ConnectAsync(config.SmtpHost, config.SmtpPort, secureOpt, cancellationToken);

                    if (!string.IsNullOrWhiteSpace(usuario))
                        await client.AuthenticateAsync(usuario, config.SmtpSenha ?? "", cancellationToken);

                    await client.SendAsync(message, cancellationToken);
                    await client.DisconnectAsync(true, cancellationToken);

                    _logger.LogInformation("E-mail enviado com sucesso para {Destinatario} (usuário: {Usuario})", destinatario, usuario ?? "(nenhum)");
                    return (true, null);
                }
                catch (Exception ex)
                {
                    ultimoErro = ex;
                    _logger.LogWarning(ex, "Tentativa falhou (usuário={Usuario}, SSL={Ssl})", usuario ?? "(nenhum)", secureOpt);
                }
            }
        }

        _logger.LogError(ultimoErro, "Erro ao enviar e-mail para {Destinatario}", destinatario);
        var msg = ultimoErro != null && !string.IsNullOrWhiteSpace(ultimoErro.Message) ? ultimoErro.Message : "Falha ao enviar.";
        if (ultimoErro?.InnerException != null && !string.IsNullOrWhiteSpace(ultimoErro.InnerException.Message))
            msg += " | " + ultimoErro.InnerException.Message;
        return (false, msg);
    }

    private static IEnumerable<string> ObterVariacoesUsuario(string? smtpUsuario)
    {
        if (string.IsNullOrWhiteSpace(smtpUsuario)) return [""];
        var list = new List<string> { smtpUsuario.Trim() };
        if (smtpUsuario.Contains('@'))
        {
            var parteLocal = smtpUsuario.Split('@')[0].Trim();
            if (!string.IsNullOrEmpty(parteLocal) && !list.Contains(parteLocal))
                list.Add(parteLocal);
        }
        return list;
    }

    private static List<SecureSocketOptions> ObterOpcoesSslParaTestar(Domain.Entities.ConfiguracaoEmail config)
    {
        var lista = new List<SecureSocketOptions>();
        if (config.UsarSsl)
        {
            if (config.SmtpPort == 465)
                lista.Add(SecureSocketOptions.SslOnConnect);
            else
                lista.Add(SecureSocketOptions.StartTls);
        }
        else
        {
            lista.Add(SecureSocketOptions.None);
        }
        return lista;
    }
}
