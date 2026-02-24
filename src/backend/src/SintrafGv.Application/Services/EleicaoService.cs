using SintrafGv.Application.DTOs;
using SintrafGv.Application.Interfaces;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Services;

public class EleicaoService : IEleicaoService
{
    private readonly IEleicaoRepository _repository;

    public EleicaoService(IEleicaoRepository repository) => _repository = repository;

    public async Task<(IReadOnlyList<EleicaoResumoDto> Itens, int Total)> ListarResumoAsync(int pagina, int porPagina, StatusEleicao? status, CancellationToken cancellationToken = default)
    {
        var skip = (pagina - 1) * porPagina;
        var itens = await _repository.ListarAsync(skip, porPagina, status, cancellationToken);
        var total = await _repository.ContarAsync(status, cancellationToken);
        var ids = itens.Select(e => e.Id).ToList();
        var votosPorId = ids.Count > 0 ? await _repository.ContarVotosPorEleicaoAsync(ids, cancellationToken) : new Dictionary<Guid, int>();
        var dtos = itens.Select(e => new EleicaoResumoDto
        {
            Id = e.Id,
            Titulo = e.Titulo,
            Tipo = e.Tipo,
            Status = e.Status,
            ArquivoAnexo = e.ArquivoAnexo,
            InicioVotacao = e.InicioVotacao,
            FimVotacao = e.FimVotacao,
            TotalPerguntas = e.Perguntas?.Count ?? 0,
            TotalVotos = votosPorId.GetValueOrDefault(e.Id, 0)
        }).ToList();
        return (dtos, total);
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
        var eleicao = new Eleicao
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            ArquivoAnexo = request.ArquivoAnexo,
            InicioVotacao = request.InicioVotacao,
            FimVotacao = request.FimVotacao,
            Tipo = request.Tipo,
            Status = StatusEleicao.Rascunho,
            ApenasAssociados = request.ApenasAssociados,
            ApenasAtivos = request.ApenasAtivos,
            BancoId = request.BancoId,
            CriadoEm = DateTime.UtcNow,
            CriadoPorId = criadoPorId
        };
        foreach (var pr in request.Perguntas.OrderBy(p => p.Ordem))
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
            foreach (var op in pr.Opcoes.OrderBy(o => o.Ordem))
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
        e.Titulo = request.Titulo;
        e.Descricao = request.Descricao;
        e.ArquivoAnexo = request.ArquivoAnexo;
        e.InicioVotacao = request.InicioVotacao;
        e.FimVotacao = request.FimVotacao;
        e.Tipo = request.Tipo;
        e.ApenasAssociados = request.ApenasAssociados;
        e.ApenasAtivos = request.ApenasAtivos;
        e.BancoId = request.BancoId;
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

        var totalHabilitados = totalVotantes; // Simplificado por enquanto

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
        var codigoComprovante = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var voto = new Voto
        {
            Id = Guid.NewGuid(),
            EleicaoId = eleicaoId,
            AssociadoId = associadoId,
            DataHoraVoto = DateTime.UtcNow,
            IpOrigem = ipOrigem,
            UserAgent = userAgent,
            CodigoComprovante = codigoComprovante
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
            EleicaoId = eleicaoId,
            CodigoComprovante = codigoComprovante,
            DataHoraVoto = voto.DataHoraVoto,
            Respostas = request.Respostas
        };
    }
}
