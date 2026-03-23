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
    public string? BancoNome { get; set; }
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
    /// <summary>Quando preenchido, a votação é restrita a associados deste banco.</summary>
    public string? BancoNome { get; set; }
    public DateTime InicioVotacao { get; set; }
    public DateTime FimVotacao { get; set; }
    public int TotalPerguntas { get; set; }
    public int TotalVotos { get; set; }
}

/// <summary>Resumo de eleição aberta para o PWA, com flags de elegibilidade do associado.</summary>
public class EleicaoAtivaDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ArquivoAnexo { get; set; }
    public TipoEleicao Tipo { get; set; }
    public StatusEleicao Status { get; set; }
    public bool ApenasAssociados { get; set; }
    public bool ApenasAtivos { get; set; }
    public string? BancoNome { get; set; }
    public DateTime InicioVotacao { get; set; }
    public DateTime FimVotacao { get; set; }
    public int TotalPerguntas { get; set; }
    public int TotalVotos { get; set; }
    public bool PodeVotar { get; set; }
    public bool JaVotou { get; set; }
}

public class CreateEleicaoRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ArquivoAnexo { get; set; }
    /// <summary>Data/hora início da votação (ISO 8601, ex: 2026-03-18T13:00:00.000Z).</summary>
    public string? InicioVotacao { get; set; }
    /// <summary>Data/hora fim da votação (ISO 8601).</summary>
    public string? FimVotacao { get; set; }
    public TipoEleicao Tipo { get; set; } = TipoEleicao.Enquete;
    public bool ApenasAssociados { get; set; } = true;
    public bool ApenasAtivos { get; set; } = true;
    public Guid? BancoId { get; set; }
    /// <summary>Quando preenchido, apenas associados deste banco podem votar.</summary>
    public string? BancoNome { get; set; }
    public List<CreatePerguntaRequest> Perguntas { get; set; } = new();
}

public class UpdateEleicaoRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ArquivoAnexo { get; set; }
    /// <summary>Data/hora início da votação (ISO 8601).</summary>
    public string? InicioVotacao { get; set; }
    /// <summary>Data/hora fim da votação (ISO 8601).</summary>
    public string? FimVotacao { get; set; }
    public TipoEleicao Tipo { get; set; }
    public bool ApenasAssociados { get; set; }
    public bool ApenasAtivos { get; set; }
    public Guid? BancoId { get; set; }
    public string? BancoNome { get; set; }
}

public class UpdateStatusEleicaoRequest
{
    public StatusEleicao Status { get; set; }
}
