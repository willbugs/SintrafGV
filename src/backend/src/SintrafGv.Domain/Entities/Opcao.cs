namespace SintrafGv.Domain.Entities;

/// <summary>
/// Opção de resposta para uma pergunta.
/// Em eleições: representa um candidato ou chapa.
/// Em enquetes: representa uma opção de resposta (sim, não, etc).
/// </summary>
public class Opcao
{
    public Guid Id { get; set; }
    
    public Guid PerguntaId { get; set; }
    public Pergunta? Pergunta { get; set; }
    
    public int Ordem { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Foto { get; set; }
    
    public Guid? AssociadoId { get; set; }
    public Associado? Associado { get; set; }
    
    public ICollection<VotoDetalhe> VotosDetalhes { get; set; } = new List<VotoDetalhe>();
}
