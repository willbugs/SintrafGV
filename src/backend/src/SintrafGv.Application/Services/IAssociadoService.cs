using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Services;

public interface IAssociadoService
{
    Task<Associado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Associado> Itens, int Total)> ListarAsync(int pagina, int porPagina, bool apenasAtivos = false, CancellationToken cancellationToken = default);
    /// <param name="status">"Ativo" | "Inativo" | null (Todos)</param>
    Task<(IReadOnlyList<Associado> Itens, int Total)> ListarAsync(int pagina, int porPagina, string? busca, string? status, CancellationToken cancellationToken = default);
    Task<Associado> CriarAsync(Associado associado, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Associado associado, CancellationToken cancellationToken = default);
}
