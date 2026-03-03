using System.ComponentModel.DataAnnotations;

namespace SintrafGv.Domain.Entities;

public class ConfiguracaoEmail
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public bool UsarSsl { get; set; } = true;

    [MaxLength(200)]
    public string? SmtpUsuario { get; set; }

    [MaxLength(500)]
    public string? SmtpSenha { get; set; }

    [MaxLength(200)]
    public string EmailRemetente { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NomeRemetente { get; set; }

    public bool Habilitado { get; set; } = false;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }
}
