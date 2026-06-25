namespace SintrafGv.Application.DTOs;

public class ResultadoEleicaoDto
{
    [ExportIgnore]
    public Guid Id { get; set; }
    [ExportIgnore]
    public Guid EleicaoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    [ExportIgnore]
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalVotos { get; set; }
    [ExportIgnore]
    public int TotalVotantes { get; set; }
    [ExportIgnore]
    public int TotalHabilitados { get; set; }
    [ExportIgnore]
    public int TotalAssociadosElegiveis { get; set; }
    public decimal PercentualParticipacao { get; set; }
    [ExportIgnore]
    public List<CandidatoResultadoDto> Candidatos { get; set; } = new();
    public string Vencedor { get; set; } = string.Empty;
    [ExportIgnore]
    public List<ResultadoPerguntaDto> Perguntas { get; set; } = new();
}

public class CandidatoResultadoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int NumeroVotos { get; set; }
    public decimal PercentualVotos { get; set; }
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
