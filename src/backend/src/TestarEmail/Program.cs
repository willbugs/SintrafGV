using System;
using System.Collections.Generic;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Data.SqlClient;

const int timeoutMs = 60000;
var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? "Data Source=127.0.0.1;Initial Catalog=Sintraf_GV;Integrated Security=false;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;User Id=Durval;Password=Lspxmw01oz";

Console.WriteLine("Testar Email - SintrafGV");
Console.WriteLine("Timeout: {0}s", timeoutMs / 1000);
Console.WriteLine();

string? smtpHost = null, smtpUsuario = null, smtpSenha = null, emailRemetente = null, nomeRemetente = null;
int smtpPort = 587;
bool usarSsl = true;

using (var conn = new SqlConnection(connStr))
{
    conn.Open();
    using var cmd = new SqlCommand("SELECT SmtpHost, SmtpPort, UsarSsl, SmtpUsuario, SmtpSenha, EmailRemetente, NomeRemetente FROM ConfiguracoesEmail", conn);
    using var r = cmd.ExecuteReader();
    if (!r.Read())
    {
        Console.WriteLine("ERRO: Nenhuma configuração de email no banco.");
        return 1;
    }
    smtpHost = r.GetString(0);
    smtpPort = r.GetInt32(1);
    usarSsl = r.GetBoolean(2);
    smtpUsuario = r.IsDBNull(3) ? null : r.GetString(3);
    smtpSenha = r.IsDBNull(4) ? null : r.GetString(4);
    emailRemetente = r.GetString(5);
    nomeRemetente = r.IsDBNull(6) ? null : r.GetString(6);
}

Console.WriteLine($"Host: {smtpHost}:{smtpPort} SSL={usarSsl}");
Console.WriteLine($"Usuario: {smtpUsuario ?? "(vazio)"}");
Console.WriteLine($"Remetente: {nomeRemetente ?? "SintrafGV"} <{emailRemetente}>");
Console.WriteLine($"Senha: {(string.IsNullOrEmpty(smtpSenha) ? "VAZIA" : $"{smtpSenha.Length} chars")}");
Console.WriteLine();

var destinatario = args.Length > 0 ? args[0] : emailRemetente;
if (string.IsNullOrWhiteSpace(destinatario))
{
    Console.WriteLine("Uso: dotnet run -- <email-destino>");
    Console.WriteLine("Ex: dotnet run -- teste@exemplo.com");
    return 1;
}

var usuarios = new List<string>();
if (!string.IsNullOrWhiteSpace(smtpUsuario))
{
    usuarios.Add(smtpUsuario.Trim());
    if (smtpUsuario.Contains('@'))
    {
        var parte = smtpUsuario.Split('@')[0].Trim();
        if (!string.IsNullOrEmpty(parte) && !usuarios.Contains(parte)) usuarios.Add(parte);
    }
}

var opcoesSsl = new List<SecureSocketOptions> { SecureSocketOptions.None };
if (usarSsl)
    opcoesSsl.Add(smtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

var message = new MimeKit.MimeMessage();
message.From.Add(new MimeKit.MailboxAddress(nomeRemetente ?? "SintrafGV", emailRemetente));
message.To.Add(MimeKit.MailboxAddress.Parse(destinatario));
message.Subject = "Teste SintrafGV - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
message.Body = new MimeKit.TextPart("plain") { Text = "Teste de envio. Configuração OK." };

foreach (var opt in opcoesSsl)
{
    foreach (var usuario in usuarios)
    {
        try
        {
            Console.WriteLine($"Tentando: {usuario ?? "(nenhum)"} | SSL={opt}...");
            using var client = new SmtpClient();
            client.Timeout = timeoutMs;
            client.ServerCertificateValidationCallback = (_, _, _, _) => true;
            client.Connect(smtpHost, smtpPort, opt, System.Threading.CancellationToken.None);
            if (!string.IsNullOrWhiteSpace(usuario))
                client.Authenticate(usuario, smtpSenha ?? "", System.Threading.CancellationToken.None);
            client.Send(message, System.Threading.CancellationToken.None);
            client.Disconnect(true, System.Threading.CancellationToken.None);
            Console.WriteLine("OK - Email enviado para " + destinatario);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("  Falhou: " + ex.Message);
        }
    }
}

Console.WriteLine("ERRO - Todas as tentativas falharam.");
return 1;
