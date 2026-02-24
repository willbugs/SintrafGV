using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SintrafGv.Application.DTOs;
using SintrafGv.Application.Services;

namespace SintrafGv.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "E-mail e senha são obrigatórios." });

        var result = await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (result == null)
            return Unauthorized(new { message = "Credenciais inválidas." });

        return Ok(new
        {
            success = true,
            message = "Login realizado com sucesso.",
            data = new
            {
                token = result.Token,
                user = new
                {
                    id = result.User.Id.ToString(),
                    name = result.User.Nome,
                    email = result.User.Email,
                    role = result.User.Role,
                },
            },
        });
    }

    /// <summary>Renovar token JWT (igual Bureau: [Authorize], claims do token atual, devolve novo token).</summary>
    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<ActionResult<object>> RefreshToken(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { success = false, message = "Token inválido." });

        var result = await _authService.RefreshAsync(userId, cancellationToken);
        if (result == null)
            return Unauthorized(new { success = false, message = "Usuário não autorizado." });

        return Ok(new
        {
            success = true,
            message = "Token renovado com sucesso.",
            data = new
            {
                token = result.Token,
                user = new
                {
                    id = result.User.Id.ToString(),
                    name = result.User.Nome,
                    email = result.User.Email,
                    role = result.User.Role,
                },
            },
        });
    }
}
