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
}
