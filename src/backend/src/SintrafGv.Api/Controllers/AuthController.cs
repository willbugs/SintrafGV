using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SintrafGv.Application.DTOs;
using SintrafGv.Application.Exceptions;
using SintrafGv.Application.Interfaces;
using SintrafGv.Application.Services;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAssociadoRepository _associadoRepository;
    private readonly IAssociadoService _associadoService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthController(
        IAuthService authService,
        IAssociadoRepository associadoRepository,
        IAssociadoService associadoService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _authService = authService;
        _associadoRepository = associadoRepository;
        _associadoService = associadoService;
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

        var cpfLimpo = new string(request.Cpf.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(cpfLimpo))
            return BadRequest(new { message = "CPF inválido." });
        var associado = await _associadoRepository.ObterPorCpfAsync(cpfLimpo, cancellationToken);

        if (associado == null)
            return Unauthorized(new { message = "Associado não encontrado." });

        var formatosData = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy" };
        if (!DateTime.TryParseExact(request.DataNascimento.Trim(), formatosData,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var dataNascInput))
            return BadRequest(new { message = "Data de nascimento inválida. Use o formato DD/MM/AAAA." });

        if (associado.DataNascimento == null ||
            associado.DataNascimento.Value.Date != dataNascInput.Date)
            return Unauthorized(new { message = "Dados de autenticação inválidos." });

        var matriculaLimpa = request.MatriculaBancaria.Trim();
        var matriculaDb = (associado.MatriculaBancaria ?? "").Trim();
        if (string.IsNullOrEmpty(matriculaDb))
            return Unauthorized(new { message = "Dados de autenticação inválidos." });
        var matriculaMatch = matriculaDb == matriculaLimpa ||
            matriculaDb.TrimStart('0') == matriculaLimpa.TrimStart('0');
        if (!matriculaMatch)
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
                ativo = associado.Ativo,
                filiado = associado.Filiado
            }
        });
    }

    /// <summary>Verifica se matrícula bancária já está cadastrada (cadastro na votação).</summary>
    [HttpGet("associado/existe-matricula")]
    public async Task<ActionResult<object>> ExisteMatriculaBancaria(
        [FromQuery] string matricula,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(matricula))
            return Ok(new { existe = false });

        var associado = await _associadoRepository.ObterPorMatriculaBancariaAsync(matricula.Trim(), cancellationToken);
        return Ok(new { existe = associado != null });
    }

    /// <summary>Cadastro público de associado no PWA de votação (equivalente ao legado WebEnquete).</summary>
    [HttpPost("associado/cadastro")]
    public async Task<ActionResult<object>> CadastroAssociadoVotacao(
        [FromBody] CadastroAssociadoVotacaoRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.AceiteTermos)
            return BadRequest(new { message = "É necessário aceitar os termos de uso." });

        if (string.IsNullOrWhiteSpace(request.Nome) ||
            string.IsNullOrWhiteSpace(request.Cpf) ||
            string.IsNullOrWhiteSpace(request.DataNascimento) ||
            string.IsNullOrWhiteSpace(request.MatriculaBancaria) ||
            string.IsNullOrWhiteSpace(request.Celular) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Banco))
            return BadRequest(new { message = "Todos os campos são obrigatórios." });

        var cpfLimpo = new string(request.Cpf.Where(char.IsDigit).ToArray());
        if (cpfLimpo.Length != 11)
            return BadRequest(new { message = "CPF inválido." });

        var formatosData = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy" };
        if (!DateTime.TryParseExact(request.DataNascimento.Trim(), formatosData,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var dataNascimento))
            return BadRequest(new { message = "Data de nascimento inválida. Use o formato DD/MM/AAAA." });

        var matriculaLimpa = request.MatriculaBancaria.Trim();
        var matriculaExistente = await _associadoRepository.ObterPorMatriculaBancariaAsync(matriculaLimpa, cancellationToken);
        if (matriculaExistente != null)
            return Conflict(new { message = "Matrícula bancária já cadastrada." });

        var cpfExistente = await _associadoRepository.ObterPorCpfAsync(cpfLimpo, cancellationToken);
        if (cpfExistente != null)
            return Conflict(new { message = "CPF já cadastrado." });

        var celularLimpo = new string(request.Celular.Where(char.IsDigit).ToArray());
        var associado = new Associado
        {
            Nome = request.Nome.Trim().ToUpperInvariant(),
            Cpf = cpfLimpo,
            DataNascimento = dataNascimento.Date,
            MatriculaBancaria = matriculaLimpa,
            Celular = string.IsNullOrEmpty(celularLimpo) ? request.Celular.Trim() : celularLimpo,
            Email = request.Email.Trim(),
            Banco = request.Banco.Trim(),
            Ativo = true,
            Filiado = false,
            Aposentado = false
        };

        try
        {
            await _associadoService.CriarAsync(associado, cancellationToken);
        }
        catch (CpfDuplicadoException)
        {
            return Conflict(new { message = "CPF já cadastrado." });
        }
        catch (ArgumentException ex) when (ex.ParamName == "associado")
        {
            return BadRequest(new { message = ex.Message });
        }

        return Ok(new { message = "Cadastro realizado com sucesso. Faça login para votar." });
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

public class CadastroAssociadoVotacaoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string DataNascimento { get; set; } = string.Empty;
    public string MatriculaBancaria { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public bool AceiteTermos { get; set; }
}
