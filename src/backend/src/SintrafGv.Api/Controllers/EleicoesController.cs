using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SintrafGv.Application.DTOs;
using SintrafGv.Application.Services;
using SintrafGv.Domain.Entities;
using System.Security.Claims;

namespace SintrafGv.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EleicoesController : ControllerBase
{
    private readonly IEleicaoService _service;

    public EleicoesController(IEleicaoService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<object>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20,
        [FromQuery] StatusEleicao? status = null,
        CancellationToken cancellationToken = default)
    {
        var (itens, total) = await _service.ListarResumoAsync(pagina, porPagina, status, cancellationToken);
        return Ok(new { itens, total });
    }

    /// <summary>Lista eleições ativas disponíveis para votação (usado pelo PWA)</summary>
    [HttpGet("ativas")]
    public async Task<ActionResult<object>> ListarAtivas(CancellationToken cancellationToken = default)
    {
        var (itens, _) = await _service.ListarResumoAsync(1, 100, StatusEleicao.Aberta, cancellationToken);
        return Ok(itens);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EleicaoDto>> ObterPorId(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await _service.ObterPorIdAsync(id, cancellationToken);
        if (dto is null)
            return NotFound();
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<EleicaoDto>> Criar([FromBody] CreateEleicaoRequest request, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) is { } s && Guid.TryParse(s, out var id) ? id : (Guid?)null;
        var dto = await _service.CriarAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Atualizar(Guid id, [FromBody] UpdateEleicaoRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _service.AtualizarAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult> AtualizarStatus(Guid id, [FromBody] UpdateStatusEleicaoRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _service.AtualizarStatusAsync(id, request.Status, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Obtém os resultados de uma eleição (apenas para eleições encerradas/apuradas)
    /// </summary>
    [HttpGet("{id:guid}/resultados")]
    public async Task<ActionResult<ResultadoEleicaoDto>> ObterResultados(Guid id, CancellationToken cancellationToken = default)
    {
        var resultados = await _service.ObterResultadosAsync(id, cancellationToken);
        if (resultados is null)
            return NotFound("Eleição não encontrada ou não está disponível para apuração.");

        return Ok(resultados);
    }

    /// <summary>
    /// Registra voto de um associado em uma eleição
    /// </summary>
    [HttpPost("{id:guid}/votar")]
    public async Task<ActionResult<VotoDto>> Votar(Guid id, [FromBody] CreateVotoRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst("AssociadoId")?.Value ?? User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var associadoId))
                return Unauthorized("Token inválido ou associado não identificado.");

            var ipOrigem = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();

            var voto = await _service.VotarAsync(id, request, associadoId, ipOrigem, userAgent, cancellationToken);
            return Ok(voto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
