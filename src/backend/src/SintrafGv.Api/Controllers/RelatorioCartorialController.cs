using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SintrafGv.Application.DTOs;
using SintrafGv.Application.Interfaces;

namespace SintrafGv.Api.Controllers
{
    [ApiController]
    [Route("api/relatorio-cartorial")]
    [Authorize]
    public class RelatorioCartorialController : ControllerBase
    {
        private readonly IRelatorioVotacaoCartorialService _relatorioService;

        public RelatorioCartorialController(IRelatorioVotacaoCartorialService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        /// <summary>
        /// Gera relatório completo de votação para cartório
        /// </summary>
        [HttpPost("gerar")]
        [Authorize(Roles = "Admin,Presidente,Secretario")]
        public async Task<ActionResult<RelatorioVotacaoCartorialDto>> GerarRelatorioCartorial(
            [FromBody] RelatorioCartorialRequest request)
        {
            try
            {
                var relatorio = await _relatorioService.GerarRelatorioCartorialAsync(request);
                return Ok(relatorio);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gera relatório em PDF para cartório
        /// </summary>
        [HttpPost("gerar-pdf")]
        [Authorize(Roles = "Admin,Presidente,Secretario")]
        public async Task<ActionResult> GerarRelatorioPDF(
            [FromBody] RelatorioCartorialRequest request)
        {
            try
            {
                var pdfBytes = await _relatorioService.GerarRelatorioPDFCartorialAsync(request);
                
                var nomeArquivo = $"relatorio_cartorial_{request.EleicaoId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                return File(pdfBytes, "application/pdf", nomeArquivo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Valida a integridade dos dados de votação
        /// </summary>
        [HttpGet("validar-integridade/{eleicaoId}")]
        [Authorize(Roles = "Admin,Presidente,Secretario")]
        public async Task<ActionResult<bool>> ValidarIntegridade(Guid eleicaoId)
        {
            try
            {
                var integridadeValida = await _relatorioService.ValidarIntegridadeVotacaoAsync(eleicaoId);
                return Ok(new { EleicaoId = eleicaoId, IntegridadeValida = integridadeValida });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao validar integridade: {ex.Message}");
            }
        }

        /// <summary>
        /// Gera hash de verificação para um relatório
        /// </summary>
        [HttpPost("gerar-hash")]
        [Authorize(Roles = "Admin,Presidente,Secretario")]
        public async Task<ActionResult<string>> GerarHashVerificacao(
            [FromBody] RelatorioVotacaoCartorialDto relatorio)
        {
            try
            {
                var hash = await _relatorioService.GerarHashVerificacaoAsync(relatorio);
                return Ok(new { Hash = hash });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao gerar hash: {ex.Message}");
            }
        }

        /// <summary>
        /// Assina digitalmente um hash de relatório
        /// </summary>
        [HttpPost("assinar-digitalmente")]
        [Authorize(Roles = "Admin,Presidente")]
        public async Task<ActionResult<string>> AssinarDigitalmente([FromBody] string hashRelatorio)
        {
            try
            {
                var assinatura = await _relatorioService.AssinarDigitalmenteRelatorioAsync(hashRelatorio);
                return Ok(new { AssinaturaDigital = assinatura });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao assinar digitalmente: {ex.Message}");
            }
        }

        /// <summary>
        /// Endpoint público para validação de relatórios
        /// </summary>
        [HttpGet("validar/{eleicaoId}")]
        [AllowAnonymous]
        public async Task<ActionResult> ValidarRelatorio(Guid eleicaoId, [FromQuery] string chave)
        {
            try
            {
                var integridadeValida = await _relatorioService.ValidarIntegridadeVotacaoAsync(eleicaoId);
                
                return Ok(new 
                { 
                    EleicaoId = eleicaoId,
                    ChaveValidacao = chave,
                    Valido = integridadeValida,
                    Status = integridadeValida ? "Relatório válido - Integridade verificada" : "Falha na validação de integridade",
                    DataValidacao = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    EleicaoId = eleicaoId,
                    ChaveValidacao = chave,
                    Valido = false,
                    Status = $"Eleição não encontrada ou erro na validação: {ex.Message}",
                    DataValidacao = DateTime.UtcNow
                });
            }
        }
    }
}