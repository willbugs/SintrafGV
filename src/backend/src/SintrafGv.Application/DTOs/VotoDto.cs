namespace SintrafGv.Application.DTOs;

public class VotoDto
{
    public Guid Id { get; set; }
    public Guid EleicaoId { get; set; }
    public Guid AssociadoId { get; set; }
    public string AssociadoNome { get; set; } = string.Empty;
    public DateTime DataHoraVoto { get; set; }
    public string? CodigoComprovante { get; set; }
}

public class RegistrarVotoRequest
{
    public Guid EleicaoId { get; set; }
    public List<VotoDetalheRequest> Respostas { get; set; } = new();
}

public class VotoDetalheRequest
{
    public Guid PerguntaId { get; set; }
    public Guid? OpcaoId { get; set; }
    public bool VotoBranco { get; set; } = false;
}

public class ComprovanteVotoDto
{
    public string Codigo { get; set; } = string.Empty;
    public DateTime DataHoraVoto { get; set; }
    public string EleicaoTitulo { get; set; } = string.Empty;
}
