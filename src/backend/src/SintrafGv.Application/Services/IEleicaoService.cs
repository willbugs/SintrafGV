using SintrafGv.Application.DTOs;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Services;

public interface IEleicaoService
{
    Task<(IReadOnlyList<EleicaoResumoDto> Itens, int Total)> ListarResumoAsync(
        int pagina, 
        int porPagina, 
        StatusEleicao? status,
        string? busca,
        DateTimeOffset? dataInicio,
        DateTimeOffset? dataFim,
        CancellationToken cancellationToken = default);
    Task<EleicaoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EleicaoDto> CriarAsync(CreateEleicaoRequest request, Guid? criadoPorId, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Guid id, UpdateEleicaoRequest request, CancellationToken cancellationToken = default);
    Task AtualizarStatusAsync(Guid id, StatusEleicao status, CancellationToken cancellationToken = default);

    // Apuração e Votação  
    Task<ResultadoEleicaoDto?> ObterResultadosAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VotoDto> VotarAsync(Guid eleicaoId, CreateVotoRequest request, Guid associadoId, string? ipOrigem, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>Lista eleições abertas com flags PodeVotar e JaVotou para o associado (uso no PWA).</summary>
    Task<IReadOnlyList<EleicaoAtivaDto>> ListarAtivasParaAssociadoAsync(Guid associadoId, CancellationToken cancellationToken = default);

    /// <summary>Obtém comprovante de voto (apenas para o associado dono do voto).</summary>
    Task<ComprovanteVotoDto?> ObterComprovanteAsync(Guid votoId, Guid associadoId, CancellationToken cancellationToken = default);
}
