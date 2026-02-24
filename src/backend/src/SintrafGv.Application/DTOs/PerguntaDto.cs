using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.DTOs;

public class PerguntaDto
{
    public Guid Id { get; set; }
    public Guid EleicaoId { get; set; }
    public int Ordem { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoPergunta Tipo { get; set; }
    public int? MaxVotos { get; set; }
    public bool PermiteBranco { get; set; }
    public List<OpcaoDto> Opcoes { get; set; } = new();
}

public class CreatePerguntaRequest
{
    public int Ordem { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoPergunta Tipo { get; set; } = TipoPergunta.UnicoVoto;
    public int? MaxVotos { get; set; }
    public bool PermiteBranco { get; set; } = true;
    public List<CreateOpcaoRequest> Opcoes { get; set; } = new();
}

public class UpdatePerguntaRequest
{
    public int Ordem { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoPergunta Tipo { get; set; }
    public int? MaxVotos { get; set; }
    public bool PermiteBranco { get; set; }
}
