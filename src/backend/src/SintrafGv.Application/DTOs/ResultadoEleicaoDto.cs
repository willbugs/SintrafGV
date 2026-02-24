namespace SintrafGv.Application.DTOs;

public class ResultadoEleicaoDto
{
    public Guid EleicaoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int TotalVotantes { get; set; }
    public int TotalHabilitados { get; set; }
    public decimal PercentualParticipacao { get; set; }
    public List<ResultadoPerguntaDto> Perguntas { get; set; } = new();
}

public class ResultadoPerguntaDto
{
    public Guid PerguntaId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int TotalVotos { get; set; }
    public int VotosBranco { get; set; }
    public List<ResultadoOpcaoDto> Opcoes { get; set; } = new();
}

public class ResultadoOpcaoDto
{
    public Guid OpcaoId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public int TotalVotos { get; set; }
    public decimal Percentual { get; set; }
}
