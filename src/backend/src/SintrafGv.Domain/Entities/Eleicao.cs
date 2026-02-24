namespace SintrafGv.Domain.Entities;

public enum TipoEleicao
{
    Enquete = 1,
    Eleicao = 2
}

public enum StatusEleicao
{
    Rascunho = 1,
    Aberta = 2,
    Encerrada = 3,
    Apurada = 4,
    Cancelada = 5
}

/// <summary>
/// Eleição ou enquete do sindicato.
/// Substitui a entidade CEnquetes do legado, com suporte a múltiplas perguntas.
/// </summary>
public class Eleicao
{
    public Guid Id { get; set; }
    
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ArquivoAnexo { get; set; }
    
    public DateTime InicioVotacao { get; set; }
    public DateTime FimVotacao { get; set; }
    
    public TipoEleicao Tipo { get; set; } = TipoEleicao.Enquete;
    public StatusEleicao Status { get; set; } = StatusEleicao.Rascunho;
    
    public bool ApenasAssociados { get; set; } = true;
    public bool ApenasAtivos { get; set; } = true;
    public Guid? BancoId { get; set; }
    
    public ICollection<Pergunta> Perguntas { get; set; } = new List<Pergunta>();
    public ICollection<Voto> Votos { get; set; } = new List<Voto>();
    
    public DateTime CriadoEm { get; set; }
    public Guid? CriadoPorId { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
