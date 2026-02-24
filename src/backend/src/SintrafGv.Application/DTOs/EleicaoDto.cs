using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.DTOs;

public class EleicaoDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ArquivoAnexo { get; set; }
    public DateTime InicioVotacao { get; set; }
    public DateTime FimVotacao { get; set; }
    public TipoEleicao Tipo { get; set; }
    public StatusEleicao Status { get; set; }
    public bool ApenasAssociados { get; set; }
    public bool ApenasAtivos { get; set; }
    public Guid? BancoId { get; set; }
    public DateTime CriadoEm { get; set; }
    public int TotalPerguntas { get; set; }
    public int TotalVotos { get; set; }
    public List<PerguntaDto> Perguntas { get; set; } = new();
}

public class EleicaoResumoDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public TipoEleicao Tipo { get; set; }
    public StatusEleicao Status { get; set; }
    public string? ArquivoAnexo { get; set; }
    public DateTime InicioVotacao { get; set; }
    public DateTime FimVotacao { get; set; }
    public int TotalPerguntas { get; set; }
    public int TotalVotos { get; set; }
}

public class CreateEleicaoRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ArquivoAnexo { get; set; }
    public DateTime InicioVotacao { get; set; }
    public DateTime FimVotacao { get; set; }
    public TipoEleicao Tipo { get; set; } = TipoEleicao.Enquete;
    public bool ApenasAssociados { get; set; } = true;
    public bool ApenasAtivos { get; set; } = true;
    public Guid? BancoId { get; set; }
    public List<CreatePerguntaRequest> Perguntas { get; set; } = new();
}

public class UpdateEleicaoRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ArquivoAnexo { get; set; }
    public DateTime InicioVotacao { get; set; }
    public DateTime FimVotacao { get; set; }
    public TipoEleicao Tipo { get; set; }
    public bool ApenasAssociados { get; set; }
    public bool ApenasAtivos { get; set; }
    public Guid? BancoId { get; set; }
}

public class UpdateStatusEleicaoRequest
{
    public StatusEleicao Status { get; set; }
}
