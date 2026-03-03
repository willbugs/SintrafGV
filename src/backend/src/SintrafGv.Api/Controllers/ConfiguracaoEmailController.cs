using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SintrafGv.Application.Interfaces;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Interfaces;

namespace SintrafGv.Api.Controllers;

[ApiController]
[Route("api/configuracao-email")]
[Authorize(Roles = "Admin")]
public class ConfiguracaoEmailController : ControllerBase
{
    private readonly IConfiguracaoEmailRepository _repository;
    private readonly IEmailService _emailService;

    public ConfiguracaoEmailController(IConfiguracaoEmailRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    [HttpGet("status")]
    public async Task<ActionResult<object>> ObterStatus()
    {
        var config = await _repository.ObterAsync();
        var habilitado = config != null && config.Habilitado && !string.IsNullOrWhiteSpace(config.SmtpHost);
        return Ok(new { habilitado });
    }

    [HttpGet]
    public async Task<ActionResult<ConfiguracaoEmailDto>> Obter()
    {
        var config = await _repository.ObterAsync();
        if (config == null)
            return Ok(new ConfiguracaoEmailDto());
        return Ok(new ConfiguracaoEmailDto
        {
            Id = config.Id,
            SmtpHost = config.SmtpHost,
            SmtpPort = config.SmtpPort,
            UsarSsl = config.UsarSsl,
            SmtpUsuario = config.SmtpUsuario,
            SmtpSenha = null,
            EmailRemetente = config.EmailRemetente,
            NomeRemetente = config.NomeRemetente,
            Habilitado = config.Habilitado
        });
    }

    [HttpPost]
    public async Task<ActionResult<ConfiguracaoEmailDto>> Salvar([FromBody] ConfiguracaoEmailDto dto)
    {
        var existente = await _repository.ObterAsync();
        var config = new ConfiguracaoEmail
        {
            SmtpHost = dto.SmtpHost ?? "",
            SmtpPort = dto.SmtpPort,
            UsarSsl = dto.UsarSsl,
            SmtpUsuario = dto.SmtpUsuario,
            SmtpSenha = !string.IsNullOrWhiteSpace(dto.SmtpSenha) ? dto.SmtpSenha : (existente?.SmtpSenha ?? ""),
            EmailRemetente = dto.EmailRemetente ?? "",
            NomeRemetente = dto.NomeRemetente,
            Habilitado = dto.Habilitado
        };
        var resultado = await _repository.SalvarAsync(config);
        return Ok(new ConfiguracaoEmailDto
        {
            Id = resultado.Id,
            SmtpHost = resultado.SmtpHost,
            SmtpPort = resultado.SmtpPort,
            UsarSsl = resultado.UsarSsl,
            SmtpUsuario = resultado.SmtpUsuario,
            SmtpSenha = null,
            EmailRemetente = resultado.EmailRemetente,
            NomeRemetente = resultado.NomeRemetente,
            Habilitado = resultado.Habilitado
        });
    }

    [HttpPost("testar")]
    public async Task<ActionResult<object>> Testar([FromQuery] string destinatario)
    {
        if (string.IsNullOrWhiteSpace(destinatario))
            return BadRequest(new { message = "Informe o e-mail para enviar o teste." });
        var (ok, erro) = await _emailService.TestarConfiguracaoAsync(destinatario.Trim());
        var mensagem = ok
            ? $"E-mail de teste enviado para {destinatario.Trim()}. Verifique sua caixa de entrada."
            : (erro ?? "Falha ao enviar. Verifique a configuração SMTP.");
        return Ok(new { sucesso = ok, mensagem });
    }
}

public class ConfiguracaoEmailDto
{
    public Guid Id { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public bool UsarSsl { get; set; } = true;
    public string? SmtpUsuario { get; set; }
    public string? SmtpSenha { get; set; }
    public string? EmailRemetente { get; set; }
    public string? NomeRemetente { get; set; }
    public bool Habilitado { get; set; }
}
