using SintrafGv.Application.DTOs;
using SintrafGv.Application.Interfaces;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository) => _repository = repository;

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _repository.ObterPorIdAsync(id, cancellationToken);

    public async Task<(IReadOnlyList<UsuarioListDto> Itens, int Total)> ListarAsync(int pagina, int porPagina, CancellationToken cancellationToken = default)
    {
        var skip = (pagina - 1) * porPagina;
        var total = await _repository.ContarAsync(cancellationToken);
        var itens = await _repository.ListarAsync(skip, porPagina, cancellationToken);
        var dtos = itens.Select(u => new UsuarioListDto(u.Id, u.Nome, u.Email, u.Role, u.Ativo, u.CriadoEm)).ToList();
        return (dtos, total);
    }

    public async Task<UsuarioListDto?> CriarAsync(CreateUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        if (await _repository.ObterPorEmailAsync(request.Email, cancellationToken) != null)
            return null;
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Email = request.Email.Trim(),
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Role = request.Role?.Trim() ?? "Admin",
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
        };
        await _repository.IncluirAsync(usuario, cancellationToken);
        return new UsuarioListDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Role, usuario.Ativo, usuario.CriadoEm);
    }

    public async Task<(UsuarioListDto? Dto, bool EmailEmUso)> AtualizarAsync(Guid id, UpdateUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        var usuario = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (usuario == null)
            return (null, false);
        if (await _repository.ObterPorEmailExcluindoIdAsync(request.Email.Trim(), id, cancellationToken) != null)
            return (null, true);
        usuario.Nome = request.Nome;
        usuario.Email = request.Email.Trim();
        usuario.Role = request.Role?.Trim() ?? usuario.Role;
        usuario.Ativo = request.Ativo;
        await _repository.AtualizarAsync(usuario, cancellationToken);
        return (new UsuarioListDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Role, usuario.Ativo, usuario.CriadoEm), false);
    }
}
