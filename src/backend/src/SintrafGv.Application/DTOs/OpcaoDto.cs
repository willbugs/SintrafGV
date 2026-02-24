namespace SintrafGv.Application.DTOs;

public class OpcaoDto
{
    public Guid Id { get; set; }
    public Guid PerguntaId { get; set; }
    public int Ordem { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Foto { get; set; }
    public Guid? AssociadoId { get; set; }
    public string? AssociadoNome { get; set; }
}

public class CreateOpcaoRequest
{
    public int Ordem { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Foto { get; set; }
    public Guid? AssociadoId { get; set; }
}

public class UpdateOpcaoRequest
{
    public int Ordem { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Foto { get; set; }
    public Guid? AssociadoId { get; set; }
}
