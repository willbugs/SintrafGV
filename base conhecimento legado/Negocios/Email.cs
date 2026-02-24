using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using MimeKit;
using SmtpClientMailKit = MailKit.Net.Smtp.SmtpClient;


namespace Negocios
{
    public class Email
    {
        private readonly string _userName;
        private readonly string _passWord;
        private readonly string _smtp;
        private readonly string _porta;
        private readonly bool _html;

        public Email(string smtp, string usuario, string senha, string porta, bool html)
        {
            _userName = usuario;
            _passWord = senha;
            _smtp = smtp;
            _porta = porta;
            _html = html;
        }

        public string Enviar(string to, string subject, string body, string arquivo, string mime, List<Stream> imagens)
        {
            EnviarEmail(to, out var erro, subject, body, _html, arquivo, mime, imagens);
            return erro;
        }

        public void EnviarMotor(string to, string subject, string body, string arquivo, string mime, List<Stream> imagens)
        {
            var obj = new MailMessage();

            obj.To.Add(to);
            obj.From = new MailAddress(_userName);
            obj.Priority = MailPriority.Normal;
            obj.IsBodyHtml = true;
            obj.Subject = subject;
            obj.Body = body;
            obj.SubjectEncoding = Encoding.GetEncoding("ISO-8859-1");
            obj.BodyEncoding = Encoding.GetEncoding("ISO-8859-1");

            if (!string.IsNullOrEmpty(arquivo)) obj.Attachments.Add(new Attachment(arquivo));
            if (imagens != null && imagens.Count > 0)
            {
                const int cont = 0;

                foreach (var inline in imagens.Select(imagem => new Attachment(imagem, $"imagem{cont}.jpg")))
                {
                    inline.ContentDisposition.Inline = true;
                    inline.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                    inline.ContentType.MediaType = mime;
                    obj.Attachments.Add(inline);
                }
            }

            EnviarMotorEmail(obj, _smtp, _porta, _userName, _passWord);
        }

        private void EnviarMotorEmail(MailMessage obj, string smtp, string porta, string conta, string senha)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(obj.From.Address));

                var builder = new BodyBuilder { HtmlBody = obj.Body };

                foreach (var anexo in obj.Attachments.ToList())
                {
                    builder.Attachments.Add(anexo.Name, anexo.ContentStream);
                }

                message.Body = builder.ToMessageBody();

                foreach (var destino in obj.To.ToList())
                {
                    message.To.Add(new MailboxAddress("", destino.Address));
                }

                foreach (var destino in obj.Bcc.ToList())
                {
                    message.Bcc.Add(new MailboxAddress("", destino.Address));
                }

                message.Subject = obj.Subject;


                using (var client = new SmtpClientMailKit())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(smtp, int.Parse(porta), false);
                    client.Authenticate(conta, senha);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception e)
            {
                var line = DateTime.Now + $" - Envio Email Fechs  - {obj.From} - {obj.To} " + e.Message;
                File.AppendAllText(@"c:\bots\log_Fechs.txt", line + Environment.NewLine);
                Console.WriteLine(e);
                throw;
            }
        }

        private void EnviarEmail(string to, out string erro, string subject, string body, bool isHtml, string arquivo, string mime, List<Stream> imagens)
        {
            var smtp = new SmtpClient();
            try
            {
                smtp.Host = _smtp;
                smtp.Port = int.Parse(_porta);
                smtp.Timeout = 10000;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(_userName, _passWord);
                var message = new MailMessage(_userName, to)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                if (!string.IsNullOrWhiteSpace(arquivo))
                {
                    var inline = new Attachment(arquivo);
                    inline.ContentDisposition.Inline = true;
                    inline.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                    inline.ContentType.MediaType = mime;
                    message.Attachments.Add(inline);
                }

                if (imagens != null)
                {
                    const int cont = 0;
                    foreach (var inline in imagens.Select(imagem => new Attachment(imagem, $"imagem{cont}.jpg")))
                    {
                        inline.ContentDisposition.Inline = true;
                        inline.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                        inline.ContentType.MediaType = mime;
                        message.Attachments.Add(inline);
                    }
                }

                smtp.Send(message);
                message.Dispose();
                smtp.Dispose();
            }
            catch (Exception e)
            {
                erro = e.Message;
                throw new Exception(e.Message, e);
            }
            erro = null;
        }
    }
}