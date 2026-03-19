using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SintrafGv.Application.DTOs;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Services;

public class EleicaoService : IEleicaoService
{
    private readonly IEleicaoRepository _repository;
    private readonly IAssociadoRepository _associadoRepository;
    private readonly IVotoRepository _votoRepository;

    public EleicaoService(IEleicaoRepository repository, IAssociadoRepository associadoRepository, IVotoRepository votoRepository)
    {
        _repository = repository;
        _associadoRepository = associadoRepository;
        _votoRepository = votoRepository;
    }

    public async Task<(IReadOnlyList<EleicaoResumoDto> Itens, int Total)> ListarResumoAsync(
        int pagina, 
        int porPagina, 
        StatusEleicao? status,
        string? busca,
        DateTimeOffset? dataInicio,
        DateTimeOffset? dataFim,
        CancellationToken cancellationToken = default)
    {
        var skip = (pagina - 1) * porPagina;
        var itens = await _repository.ListarAsync(skip, porPagina, status, busca, dataInicio, dataFim, cancellationToken);
        var total = await _repository.ContarAsync(status, busca, dataInicio, dataFim, cancellationToken);
        var ids = itens.Select(e => e.Id).ToList();
        var votosPorId = ids.Count > 0 ? await _repository.ContarVotosPorEleicaoAsync(ids, cancellationToken) : new Dictionary<Guid, int>();
        var dtos = itens.Select(e => new EleicaoResumoDto
        {
            Id = e.Id,
            Titulo = e.Titulo,
            Tipo = e.Tipo,
            Status = e.Status,
            ArquivoAnexo = e.ArquivoAnexo,
            BancoNome = e.BancoNome,
            InicioVotacao = e.InicioVotacao,
            FimVotacao = e.FimVotacao,
            TotalPerguntas = e.Perguntas?.Count ?? 0,
            TotalVotos = votosPorId.GetValueOrDefault(e.Id, 0)
        }).ToList();
        return (dtos, total);
    }

    public async Task<IReadOnlyList<EleicaoAtivaDto>> ListarAtivasParaAssociadoAsync(Guid associadoId, CancellationToken cancellationToken = default)
    {
        var itens = await _repository.ListarAsync(0, 100, StatusEleicao.Aberta, null, null, null, cancellationToken);
        var ids = itens.Select(e => e.Id).ToList();
        var votosPorId = ids.Count > 0 ? await _repository.ContarVotosPorEleicaoAsync(ids, cancellationToken) : new Dictionary<Guid, int>();
        var now = DateTime.UtcNow;
        var associado = await _associadoRepository.ObterPorIdAsync(associadoId, cancellationToken);
        var bancoAssociado = associado?.Banco?.Trim() ?? "";
        var result = new List<EleicaoAtivaDto>();
        foreach (var e in itens)
        {
            if (!BancoCompativelComAssociado(e.BancoNome, bancoAssociado))
                continue;
            var jaVotou = await _repository.AssociadoJaVotouAsync(e.Id, associadoId, cancellationToken);
            var dentroPeriodo = now >= e.InicioVotacao && now <= e.FimVotacao;
            var podeVotar = dentroPeriodo && !jaVotou;
            result.Add(new EleicaoAtivaDto
            {
                Id = e.Id,
                Titulo = e.Titulo,
                Descricao = e.Descricao,
                Tipo = e.Tipo,
                Status = e.Status,
                InicioVotacao = e.InicioVotacao,
                FimVotacao = e.FimVotacao,
                TotalPerguntas = e.Perguntas?.Count ?? 0,
                TotalVotos = votosPorId.GetValueOrDefault(e.Id, 0),
                PodeVotar = podeVotar,
                JaVotou = jaVotou
            });
        }
        return result;
    }

    private static bool BancoCompativelComAssociado(string? bancoEnquete, string bancoAssociado)
    {
        if (string.IsNullOrWhiteSpace(bancoEnquete)) return true;
        return string.Equals(bancoAssociado, bancoEnquete.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ComprovanteVotoDto?> ObterComprovanteAsync(Guid votoId, Guid associadoId, CancellationToken cancellationToken = default)
    {
        var voto = await _votoRepository.ObterVotoPorIdComEleicaoAsync(votoId, cancellationToken);
        if (voto is null || voto.AssociadoId != associadoId)
            return null;
        return new ComprovanteVotoDto
        {
            Id = voto.Id,
            EleicaoTitulo = voto.Eleicao?.Titulo ?? string.Empty,
            DataHoraVoto = voto.DataHoraVoto,
            HashVoto = voto.HashVoto ?? string.Empty,
            NumeroComprovante = voto.CodigoComprovante ?? string.Empty,
            AssociadoNome = voto.Associado?.Nome ?? string.Empty,
            TotalPerguntas = voto.Eleicao?.Perguntas?.Count ?? 0
        };
    }

    public async Task<EleicaoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var e = await _repository.ObterPorIdComPerguntasAsync(id, cancellationToken);
        if (e is null) return null;
        var totalVotos = (await _repository.ContarVotosPorEleicaoAsync(new[] { id }, cancellationToken)).GetValueOrDefault(id, 0);
        return ToDto(e, totalVotos);
    }

    public async Task<EleicaoDto> CriarAsync(CreateEleicaoRequest request, Guid? criadoPorId, CancellationToken cancellationToken = default)
    {
        var inicio = ParseDataVotacao(request.InicioVotacao, nameof(request.InicioVotacao));
        var fim = ParseDataVotacao(request.FimVotacao, nameof(request.FimVotacao));
        if (fim <= inicio)
            throw new InvalidOperationException("A data/hora de fim da votação deve ser posterior à data/hora de início.");
        var eleicao = new Eleicao
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            ArquivoAnexo = request.ArquivoAnexo,
            InicioVotacao = inicio,
            FimVotacao = fim,
            Tipo = request.Tipo,
            Status = StatusEleicao.Rascunho,
            ApenasAssociados = request.ApenasAssociados,
            ApenasAtivos = request.ApenasAtivos,
            BancoId = request.BancoId,
            BancoNome = string.IsNullOrWhiteSpace(request.BancoNome) ? null : request.BancoNome.Trim(),
            CriadoEm = DateTime.UtcNow,
            CriadoPorId = criadoPorId
        };
        foreach (var pr in (request.Perguntas ?? new List<CreatePerguntaRequest>()).OrderBy(p => p.Ordem))
        {
            var pergunta = new Pergunta
            {
                Id = Guid.NewGuid(),
                EleicaoId = eleicao.Id,
                Ordem = pr.Ordem,
                Texto = pr.Texto,
                Descricao = pr.Descricao,
                Tipo = pr.Tipo,
                MaxVotos = pr.MaxVotos,
                PermiteBranco = pr.PermiteBranco
            };
            foreach (var op in (pr.Opcoes ?? new List<CreateOpcaoRequest>()).OrderBy(o => o.Ordem))
            {
                pergunta.Opcoes.Add(new Opcao
                {
                    Id = Guid.NewGuid(),
                    PerguntaId = pergunta.Id,
                    Ordem = op.Ordem,
                    Texto = op.Texto,
                    Descricao = op.Descricao,
                    Foto = op.Foto,
                    AssociadoId = op.AssociadoId
                });
            }
            eleicao.Perguntas.Add(pergunta);
        }
        var criada = await _repository.IncluirAsync(eleicao, cancellationToken);
        return ToDto(criada, 0);
    }

    public async Task AtualizarAsync(Guid id, UpdateEleicaoRequest request, CancellationToken cancellationToken = default)
    {
        var e = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (e is null) throw new InvalidOperationException("Eleição não encontrada.");
        var inicio = ParseDataVotacao(request.InicioVotacao, nameof(request.InicioVotacao));
        var fim = ParseDataVotacao(request.FimVotacao, nameof(request.FimVotacao));
        if (fim <= inicio)
            throw new InvalidOperationException("A data/hora de fim da votação deve ser posterior à data/hora de início.");
        e.Titulo = request.Titulo;
        e.Descricao = request.Descricao;
        e.ArquivoAnexo = request.ArquivoAnexo;
        e.InicioVotacao = inicio;
        e.FimVotacao = fim;
        e.Tipo = request.Tipo;
        e.ApenasAssociados = request.ApenasAssociados;
        e.ApenasAtivos = request.ApenasAtivos;
        e.BancoId = request.BancoId;
        e.BancoNome = string.IsNullOrWhiteSpace(request.BancoNome) ? null : request.BancoNome.Trim();
        await _repository.AtualizarAsync(e, cancellationToken);
    }

    public async Task AtualizarStatusAsync(Guid id, StatusEleicao status, CancellationToken cancellationToken = default)
    {
        var e = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (e is null) throw new InvalidOperationException("Eleição não encontrada.");
        e.Status = status;
        await _repository.AtualizarAsync(e, cancellationToken);
    }

    private static EleicaoDto ToDto(Eleicao e, int totalVotos)
    {
        var dto = new EleicaoDto
        {
            Id = e.Id,
            Titulo = e.Titulo,
            Descricao = e.Descricao,
            ArquivoAnexo = e.ArquivoAnexo,
            InicioVotacao = e.InicioVotacao,
            FimVotacao = e.FimVotacao,
            Tipo = e.Tipo,
            Status = e.Status,
            ApenasAssociados = e.ApenasAssociados,
            ApenasAtivos = e.ApenasAtivos,
            BancoId = e.BancoId,
            BancoNome = e.BancoNome,
            CriadoEm = e.CriadoEm,
            TotalPerguntas = e.Perguntas?.Count ?? 0,
            TotalVotos = totalVotos
        };
        if (e.Perguntas != null)
        {
            foreach (var p in e.Perguntas.OrderBy(x => x.Ordem))
            {
                dto.Perguntas.Add(new PerguntaDto
                {
                    Id = p.Id,
                    EleicaoId = p.EleicaoId,
                    Ordem = p.Ordem,
                    Texto = p.Texto,
                    Descricao = p.Descricao,
                    Tipo = p.Tipo,
                    MaxVotos = p.MaxVotos,
                    PermiteBranco = p.PermiteBranco,
                    Opcoes = p.Opcoes?.OrderBy(o => o.Ordem).Select(o => new OpcaoDto
                    {
                        Id = o.Id,
                        PerguntaId = o.PerguntaId,
                        Ordem = o.Ordem,
                        Texto = o.Texto,
                        Descricao = o.Descricao,
                        Foto = o.Foto,
                        AssociadoId = o.AssociadoId,
                        AssociadoNome = o.Associado?.Nome
                    }).ToList() ?? new List<OpcaoDto>()
                });
            }
        }
        return dto;
    }

    // APURAÇÃO DE RESULTADOS
    public async Task<ResultadoEleicaoDto?> ObterResultadosAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eleicao = await _repository.ObterPorIdComPerguntasAsync(id, cancellationToken);
        if (eleicao is null) return null;

        // Só permite apuração de eleições encerradas ou apuradas
        if (eleicao.Status != StatusEleicao.Encerrada && eleicao.Status != StatusEleicao.Apurada)
            return null;

        var totalVotantes = await _repository.ContarVotantesPorEleicaoAsync(id, cancellationToken);
        var votosPorOpcao = await _repository.ContarVotosPorOpcaoAsync(id, cancellationToken);
        var votosBrancoPorPergunta = await _repository.ContarVotosBrancoPorPerguntaAsync(id, cancellationToken);

        var perguntas = eleicao.Perguntas.Select(p =>
        {
            var totalVotosPergunta = p.Opcoes.Sum(o => votosPorOpcao.GetValueOrDefault(o.Id, 0)) + 
                                     votosBrancoPorPergunta.GetValueOrDefault(p.Id, 0);

            return new ResultadoPerguntaDto
            {
                PerguntaId = p.Id,
                Texto = p.Texto,
                TotalVotos = totalVotosPergunta,
                VotosBranco = votosBrancoPorPergunta.GetValueOrDefault(p.Id, 0),
                Opcoes = p.Opcoes.Select(o =>
                {
                    var votosOpcao = votosPorOpcao.GetValueOrDefault(o.Id, 0);
                    return new ResultadoOpcaoDto
                    {
                        OpcaoId = o.Id,
                        Texto = o.Texto,
                        Foto = o.Foto,
                        TotalVotos = votosOpcao,
                        Percentual = totalVotosPergunta > 0 ? (decimal)votosOpcao / totalVotosPergunta * 100 : 0
                    };
                }).OrderByDescending(o => o.TotalVotos).ToList()
            };
        }).OrderBy(p => p.PerguntaId).ToList();

        var totalHabilitados = await _associadoRepository.ContarAssociadosAtivosAsync(cancellationToken);

        return new ResultadoEleicaoDto
        {
            EleicaoId = eleicao.Id,
            Titulo = eleicao.Titulo,
            TotalVotantes = totalVotantes,
            TotalHabilitados = totalHabilitados,
            PercentualParticipacao = totalHabilitados > 0 ? (decimal)totalVotantes / totalHabilitados * 100 : 0,
            Perguntas = perguntas
        };
    }

    // VOTAÇÃO
    public async Task<VotoDto> VotarAsync(Guid eleicaoId, CreateVotoRequest request, Guid associadoId, string? ipOrigem, string? userAgent, CancellationToken cancellationToken = default)
    {
        var eleicao = await _repository.ObterPorIdComPerguntasAsync(eleicaoId, cancellationToken);
        if (eleicao is null)
            throw new InvalidOperationException("Eleição não encontrada.");

        // Validações
        if (eleicao.Status != StatusEleicao.Aberta)
            throw new InvalidOperationException("Eleição não está aberta para votação.");

        if (DateTime.UtcNow < eleicao.InicioVotacao || DateTime.UtcNow > eleicao.FimVotacao)
            throw new InvalidOperationException("Eleição fora do período de votação.");

        if (await _repository.AssociadoJaVotouAsync(eleicaoId, associadoId, cancellationToken))
            throw new InvalidOperationException("Associado já votou nesta eleição.");

        if (!string.IsNullOrWhiteSpace(eleicao.BancoNome))
        {
            var associado = await _associadoRepository.ObterPorIdAsync(associadoId, cancellationToken);
            var bancoAssociado = associado?.Banco?.Trim() ?? "";
            if (!BancoCompativelComAssociado(eleicao.BancoNome, bancoAssociado))
                throw new InvalidOperationException($"Esta votação é restrita ao banco {eleicao.BancoNome}. Seu cadastro está vinculado a outro banco.");
        }

        // Validar respostas
        var respostasAgrupadas = request.Respostas.GroupBy(r => r.PerguntaId).ToList();

        foreach (var grupo in respostasAgrupadas)
        {
            var perguntaId = grupo.Key;
            var pergunta = eleicao.Perguntas.FirstOrDefault(p => p.Id == perguntaId);
            
            if (pergunta is null)
                throw new InvalidOperationException($"Pergunta {perguntaId} não pertence a esta eleição.");

            var respostas = grupo.ToList();
            
            // Validar limites de votos
            if (pergunta.Tipo == TipoPergunta.UnicoVoto && respostas.Count > 1)
                throw new InvalidOperationException($"Pergunta '{pergunta.Texto}' permite apenas um voto.");

            if (pergunta.Tipo == TipoPergunta.MultiploVoto && pergunta.MaxVotos.HasValue && respostas.Count > pergunta.MaxVotos.Value)
                throw new InvalidOperationException($"Pergunta '{pergunta.Texto}' permite no máximo {pergunta.MaxVotos} votos.");

            // Validar opções
            var opcoesIds = pergunta.Opcoes.Select(o => o.Id).ToHashSet();
            foreach (var resposta in respostas)
            {
                if (resposta.OpcaoId.HasValue && !opcoesIds.Contains(resposta.OpcaoId.Value))
                    throw new InvalidOperationException($"Opção {resposta.OpcaoId} não pertence à pergunta '{pergunta.Texto}'.");
            }
        }

        // Criar voto
        var dataHora = DateTime.UtcNow;
        var codigoComprovante = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var payloadHash = $"{eleicaoId}:{associadoId}:{dataHora:O}:{codigoComprovante}";
        var hashVoto = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payloadHash)));
        var voto = new Voto
        {
            Id = Guid.NewGuid(),
            EleicaoId = eleicaoId,
            AssociadoId = associadoId,
            DataHoraVoto = dataHora,
            IpOrigem = ipOrigem,
            UserAgent = userAgent,
            CodigoComprovante = codigoComprovante,
            HashVoto = hashVoto,
            TimestampPreciso = dataHora.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
            RespostaCriptografada = string.Empty,
            ChaveCriptografia = string.Empty
        };

        var detalhes = request.Respostas.Select(r => new VotoDetalhe
        {
            Id = Guid.NewGuid(),
            PerguntaId = r.PerguntaId,
            OpcaoId = r.OpcaoId,
            DataHora = DateTime.UtcNow,
            VotoBranco = !r.OpcaoId.HasValue || r.VotoBranco
        }).ToList();

        await _repository.RegistrarVotoAsync(voto, detalhes, cancellationToken);

        return new VotoDto
        {
            Id = voto.Id,
            EleicaoId = eleicaoId,
            CodigoComprovante = codigoComprovante,
            DataHoraVoto = voto.DataHoraVoto,
            Respostas = request.Respostas
        };
    }

    /// <summary>Converte string ISO 8601 (ex: 2026-03-18T13:00:00.000Z) em DateTime (UTC).</summary>
    private static DateTime ParseDataVotacao(string? value, string nomeCampo)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Data de votação inválida: {nomeCampo} é obrigatório.", nomeCampo);
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            return dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
        throw new ArgumentException($"Data de votação inválida: {nomeCampo} deve estar em formato ISO 8601 (ex: 2026-03-18T13:00:00.000Z).", nomeCampo);
    }
}
