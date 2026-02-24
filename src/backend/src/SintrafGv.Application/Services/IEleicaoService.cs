using SintrafGv.Application.DTOs;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Services;

public interface IEleicaoService
{
    Task<(IReadOnlyList<EleicaoResumoDto> Itens, int Total)> ListarResumoAsync(int pagina, int porPagina, StatusEleicao? status, CancellationToken cancellationToken = default);
    Task<EleicaoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EleicaoDto> CriarAsync(CreateEleicaoRequest request, Guid? criadoPorId, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Guid id, UpdateEleicaoRequest request, CancellationToken cancellationToken = default);
    Task AtualizarStatusAsync(Guid id, StatusEleicao status, CancellationToken cancellationToken = default);
}
