namespace SintrafGv.Domain.Entities;

/// <summary>
/// Registro do voto em cada pergunta.
/// VotoId liga a escolha ao registro de participação (visível apenas para staff/admin).
/// Associado votou em branco: OpcaoId é null.
/// </summary>
public class VotoDetalhe
{
    public Guid Id { get; set; }

    public Guid? VotoId { get; set; }
    public Voto? Voto { get; set; }
    
    public Guid PerguntaId { get; set; }
    public Pergunta? Pergunta { get; set; }
    
    public Guid? OpcaoId { get; set; }
    public Opcao? Opcao { get; set; }
    
    public DateTime DataHora { get; set; }
    
    public bool VotoBranco { get; set; } = false;
}
