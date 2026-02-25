using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SintrafGv.Application.DTOs;
using SintrafGv.Application.Interfaces;
using SintrafGv.Application.Services;
using SintrafGv.Domain.Interfaces;

namespace SintrafGv.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAssociadoRepository _associadoRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthController(
        IAuthService authService,
        IAssociadoRepository associadoRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _authService = authService;
        _associadoRepository = associadoRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

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

    /// <summary>Login para associados via CPF + Data Nascimento + Matrícula Bancária (PWA de Votação)</summary>
    [HttpPost("associado/login")]
    public async Task<ActionResult<object>> LoginAssociado(
        [FromBody] LoginAssociadoRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Cpf) ||
            string.IsNullOrWhiteSpace(request.DataNascimento) ||
            string.IsNullOrWhiteSpace(request.MatriculaBancaria))
            return BadRequest(new { message = "CPF, data de nascimento e matrícula bancária são obrigatórios." });

        var cpfLimpo = request.Cpf.Replace(".", "").Replace("-", "").Trim();
        var associado = await _associadoRepository.ObterPorCpfAsync(cpfLimpo, cancellationToken);

        if (associado == null || !associado.Ativo)
            return Unauthorized(new { message = "Associado não encontrado ou inativo." });

        if (!DateTime.TryParse(request.DataNascimento, out var dataNascInput))
            return BadRequest(new { message = "Data de nascimento inválida." });

        if (associado.DataNascimento == null ||
            associado.DataNascimento.Value.Date != dataNascInput.Date)
            return Unauthorized(new { message = "Dados de autenticação inválidos." });

        var matriculaLimpa = request.MatriculaBancaria.Trim();
        if (string.IsNullOrEmpty(associado.MatriculaBancaria) ||
            associado.MatriculaBancaria.Trim() != matriculaLimpa)
            return Unauthorized(new { message = "Dados de autenticação inválidos." });

        var token = _jwtTokenGenerator.GenerateTokenAssociado(associado);

        return Ok(new
        {
            token,
            associado = new
            {
                id = associado.Id.ToString(),
                nome = associado.Nome,
                cpf = associado.Cpf,
                email = associado.Email,
                ativo = associado.Ativo
            }
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

    /// <summary>Alterar senha do usuário autenticado</summary>
    [HttpPost("alterar-senha")]
    [Authorize]
    public async Task<ActionResult<object>> AlterarSenha(
        [FromBody] AlterarSenhaRequest request, 
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Token inválido." });

        if (string.IsNullOrWhiteSpace(request.SenhaAtual) || string.IsNullOrWhiteSpace(request.NovaSenha))
            return BadRequest(new { message = "Senha atual e nova senha são obrigatórias." });

        if (request.NovaSenha.Length < 6)
            return BadRequest(new { message = "Nova senha deve ter pelo menos 6 caracteres." });

        var result = await _authService.AlterarSenhaAsync(userId, request.SenhaAtual, request.NovaSenha, cancellationToken);
        
        if (!result)
            return BadRequest(new { message = "Senha atual incorreta." });

        return Ok(new { success = true, message = "Senha alterada com sucesso." });
    }
}

public class AlterarSenhaRequest
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}

public class LoginAssociadoRequest
{
    public string Cpf { get; set; } = string.Empty;
    public string DataNascimento { get; set; } = string.Empty;
    public string MatriculaBancaria { get; set; } = string.Empty;
}
