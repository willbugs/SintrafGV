namespace SintrafGv.Domain.Entities;

/// <summary>
/// Registro anônimo do voto em cada pergunta.
/// Não possui vínculo com Voto nem Associado para garantir sigilo.
/// Associado votou em branco: OpcaoId é null.
/// </summary>
public class VotoDetalhe
{
    public Guid Id { get; set; }
    
    public Guid PerguntaId { get; set; }
    public Pergunta? Pergunta { get; set; }
    
    public Guid? OpcaoId { get; set; }
    public Opcao? Opcao { get; set; }
    
    public DateTime DataHora { get; set; }
    
    public bool VotoBranco { get; set; } = false;
}
