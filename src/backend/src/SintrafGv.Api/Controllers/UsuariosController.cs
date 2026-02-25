using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SintrafGv.Application.DTOs;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Application.Interfaces;

namespace SintrafGv.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service) => _service = service;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioListDto>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _service.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
            return NotFound();
        return Ok(new UsuarioListDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Role, usuario.Ativo, usuario.CriadoEm));
    }

    [HttpGet]
    public async Task<ActionResult<object>> Listar([FromQuery] int pagina = 1, [FromQuery] int porPagina = 20, CancellationToken cancellationToken = default)
    {
        var (itens, total) = await _service.ListarAsync(pagina, porPagina, cancellationToken);
        return Ok(new { itens, total });
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioListDto>> Criar([FromBody] CreateUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            return BadRequest(new { message = "E-mail e senha são obrigatórios." });
        var criado = await _service.CriarAsync(request, cancellationToken);
        if (criado is null)
            return Conflict(new { message = "Já existe usuário com este e-mail." });
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UsuarioListDto>> Atualizar(Guid id, [FromBody] UpdateUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        var (dto, emailEmUso) = await _service.AtualizarAsync(id, request, cancellationToken);
        if (emailEmUso)
            return Conflict(new { message = "E-mail já utilizado por outro usuário." });
        if (dto is null)
            return NotFound();
        return Ok(dto);
    }

    [HttpGet("{id:guid}/historico-acoes")]
    public ActionResult<List<object>> ObterHistoricoAcoes(Guid id, [FromQuery] int limite = 10)
    {
        // Retorna lista vazia - funcionalidade será implementada futuramente
        // Requer criação da tabela HistoricoAcoesUsuario
        return Ok(new List<object>());
    }
}
