using SintrafGv.Application.DTOs;
using SintrafGv.Application.Interfaces;

namespace SintrafGv.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUsuarioRepository usuarioRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _usuarioRepository = usuarioRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(email, cancellationToken);
        if (usuario == null || !usuario.Ativo)
            return null;
        if (!BCrypt.Net.BCrypt.Verify(password, usuario.SenhaHash))
            return null;
        var token = _jwtTokenGenerator.GenerateToken(usuario);
        var userDto = new UserDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Role);
        return new LoginResponse(token, userDto);
    }

    public async Task<LoginResponse?> RefreshAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(userId, cancellationToken);
        if (usuario == null || !usuario.Ativo)
            return null;
        var token = _jwtTokenGenerator.GenerateToken(usuario);
        var userDto = new UserDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Role);
        return new LoginResponse(token, userDto);
    }
}
