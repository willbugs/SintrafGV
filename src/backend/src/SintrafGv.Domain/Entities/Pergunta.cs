namespace SintrafGv.Domain.Entities;

public enum TipoPergunta
{
    UnicoVoto = 1,
    MultiploVoto = 2
}

/// <summary>
/// Pergunta dentro de uma eleição.
/// Pode ser um cargo (Presidente, Conselho Fiscal) ou uma questão de enquete.
/// </summary>
public class Pergunta
{
    public Guid Id { get; set; }
    
    public Guid EleicaoId { get; set; }
    public Eleicao? Eleicao { get; set; }
    
    public int Ordem { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    
    public TipoPergunta Tipo { get; set; } = TipoPergunta.UnicoVoto;
    public int? MaxVotos { get; set; }
    public bool PermiteBranco { get; set; } = true;
    
    public ICollection<Opcao> Opcoes { get; set; } = new List<Opcao>();
    public ICollection<VotoDetalhe> VotosDetalhes { get; set; } = new List<VotoDetalhe>();
}
