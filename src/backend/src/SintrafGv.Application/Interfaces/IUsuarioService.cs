using SintrafGv.Application.DTOs;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<UsuarioListDto> Itens, int Total)> ListarAsync(int pagina, int porPagina, CancellationToken cancellationToken = default);
    Task<UsuarioListDto?> CriarAsync(CreateUsuarioRequest request, CancellationToken cancellationToken = default);
    /// <returns>Dto se ok, null se não encontrado, (null, true) se e-mail já em uso por outro</returns>
    Task<(UsuarioListDto? Dto, bool EmailEmUso)> AtualizarAsync(Guid id, UpdateUsuarioRequest request, CancellationToken cancellationToken = default);
}
