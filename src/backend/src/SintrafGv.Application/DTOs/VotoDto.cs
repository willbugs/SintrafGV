namespace SintrafGv.Application.DTOs;

public class VotoDto
{
    public Guid EleicaoId { get; set; }
    public DateTime DataHoraVoto { get; set; }
    public string? CodigoComprovante { get; set; }
    public List<VotoDetalheRequest> Respostas { get; set; } = new();
}

public class CreateVotoRequest
{
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
