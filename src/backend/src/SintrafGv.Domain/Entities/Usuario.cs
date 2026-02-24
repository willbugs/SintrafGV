namespace SintrafGv.Domain.Entities;

/// <summary>Usuário do sistema (login no painel). Não confundir com Associado (sócio do sindicato).</summary>
public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
}
