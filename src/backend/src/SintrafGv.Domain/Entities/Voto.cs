namespace SintrafGv.Domain.Entities;

/// <summary>
/// Registro de que um associado votou em uma eleição.
/// NÃO armazena em quem votou, apenas que participou.
/// Garante que cada associado vote apenas uma vez por eleição.
/// </summary>
public class Voto
{
    public Guid Id { get; set; }
    
    public Guid EleicaoId { get; set; }
    public Eleicao? Eleicao { get; set; }
    
    public Guid AssociadoId { get; set; }
    public Associado? Associado { get; set; }
    
    public DateTime DataHoraVoto { get; set; }
    public string? IpOrigem { get; set; }
    public string? UserAgent { get; set; }
    
    public string? CodigoComprovante { get; set; }
}
