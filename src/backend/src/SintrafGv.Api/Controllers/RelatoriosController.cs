using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SintrafGv.Application.DTOs;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Application.Interfaces;

namespace SintrafGv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RelatoriosController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;

        public RelatoriosController(IRelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        /// <summary>
        /// Obtém dados para o dashboard principal
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardKpiDto>> ObterDashboard(CancellationToken cancellationToken)
        {
            var kpis = await _relatorioService.ObterDashboardKpisAsync(cancellationToken);
            return Ok(kpis);
        }

        /// <summary>
        /// Relatório geral de associados
        /// </summary>
        [HttpPost("associados/geral")]
        public async Task<ActionResult<RelatorioResponse<AssociadoRelatorioDto>>> RelatorioAssociadosGeral(
            RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var resultado = await _relatorioService.ObterRelatorioAssociadosGeralAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// Relatório de associados ativos
        /// </summary>
        [HttpPost("associados/ativos")]
        public async Task<ActionResult<RelatorioResponse<AssociadoRelatorioDto>>> RelatorioAssociadosAtivos(
            RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var resultado = await _relatorioService.ObterRelatorioAssociadosAtivosAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// Relatório de associados inativos
        /// </summary>
        [HttpPost("associados/inativos")]
        public async Task<ActionResult<RelatorioResponse<AssociadoRelatorioDto>>> RelatorioAssociadosInativos(
            RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var resultado = await _relatorioService.ObterRelatorioAssociadosInativosAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// Relatório de aniversariantes
        /// </summary>
        [HttpPost("associados/aniversariantes")]
        public async Task<ActionResult<RelatorioResponse<AssociadoRelatorioDto>>> RelatorioAniversariantes(
            RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var resultado = await _relatorioService.ObterRelatorioAniversariantesAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// Relatório de novos associados
        /// </summary>
        [HttpPost("associados/novos")]
        public async Task<ActionResult<RelatorioResponse<AssociadoRelatorioDto>>> RelatorioNovosAssociados(
            RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var resultado = await _relatorioService.ObterRelatorioNovosAssociadosAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// Relatório por sexo
        /// </summary>
        [HttpPost("associados/por-sexo")]
        public async Task<ActionResult<RelatorioResponse<AssociadoRelatorioDto>>> RelatorioPorSexo(
            RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var resultado = await _relatorioService.ObterRelatorioPorSexoAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// Relatório por banco
        /// </summary>
        [HttpPost("associados/por-banco")]
        public async Task<ActionResult<RelatorioResponse<AssociadoRelatorioDto>>> RelatorioPorBanco(
            RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var resultado = await _relatorioService.ObterRelatorioPorBancoAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// Relatório por cidade
        /// </summary>
        [HttpPost("associados/por-cidade")]
        public async Task<ActionResult<RelatorioResponse<AssociadoRelatorioDto>>> RelatorioPorCidade(
            RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var resultado = await _relatorioService.ObterRelatorioPorCidadeAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// Obtém campos disponíveis para um tipo de relatório
        /// </summary>
        [HttpGet("campos/{tipoRelatorio}")]
        public async Task<ActionResult<CampoRelatorio[]>> ObterCamposDisponiveis(
            string tipoRelatorio, 
            CancellationToken cancellationToken)
        {
            var campos = await _relatorioService.ObterCamposDisponiveisAsync(tipoRelatorio, cancellationToken);
            return Ok(campos);
        }

        /// <summary>
        /// Obtém tipos de relatório disponíveis
        /// </summary>
        [HttpGet("tipos")]
        public async Task<ActionResult<string[]>> ObterTiposRelatorio(CancellationToken cancellationToken)
        {
            var tipos = await _relatorioService.ObterTiposRelatorioAsync(cancellationToken);
            return Ok(tipos);
        }

        /// <summary>
        /// Exporta relatório em formato específico
        /// </summary>
        [HttpPost("exportar")]
        public async Task<ActionResult> ExportarRelatorio(
            RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var arquivo = await _relatorioService.ExportarRelatorioAsync(request, cancellationToken);
                return File(arquivo.Conteudo, arquivo.ContentType, arquivo.NomeArquivo);
            }
            catch (NotImplementedException)
            {
                return BadRequest(new { message = "Funcionalidade de exportação será implementada na próxima fase" });
            }
        }

        /// <summary>
        /// Executa relatório customizado
        /// </summary>
        [HttpPost("customizado")]
        public async Task<ActionResult<RelatorioResponse<dynamic>>> RelatorioCustomizado(
            RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var resultado = await _relatorioService.ExecutarRelatorioCustomizadoAsync(request, cancellationToken);
                return Ok(resultado);
            }
            catch (NotImplementedException)
            {
                return BadRequest(new { message = "Relatórios customizados serão implementados na próxima fase" });
            }
        }

        // === RELATÓRIOS ESPECÍFICOS DE GESTÃO SINDICAL ===

        /// <summary>
        /// Relatório de Participação em Votações - análise de engajamento
        /// </summary>
        [HttpPost("participacao-votacao")]
        public async Task<ActionResult<RelatorioResponse<ParticipacaoVotacaoDto>>> RelatorioParticipacaoVotacao(
            [FromBody] RelatorioRequest request,
            CancellationToken cancellationToken)
        {
            var relatorio = await _relatorioService.ObterRelatorioParticipacaoVotacaoAsync(request, cancellationToken);
            return Ok(relatorio);
        }

        /// <summary>
        /// Relatório de Resultados de Eleições - detalhamento por eleição
        /// </summary>
        [HttpPost("resultados-eleicao")]
        public async Task<ActionResult<RelatorioResponse<ResultadoEleicaoDto>>> RelatorioResultadosEleicao(
            [FromBody] RelatorioRequest request,
            CancellationToken cancellationToken)
        {
            var relatorio = await _relatorioService.ObterRelatorioResultadosEleicaoAsync(request, cancellationToken);
            return Ok(relatorio);
        }

        /// <summary>
        /// Relatório de Engajamento em Votações - métricas por período
        /// </summary>
        [HttpPost("engajamento-votacao")]
        public async Task<ActionResult<RelatorioResponse<EngajamentoVotacaoDto>>> RelatorioEngajamentoVotacao(
            [FromBody] RelatorioRequest request,
            CancellationToken cancellationToken)
        {
            var relatorio = await _relatorioService.ObterRelatorioEngajamentoVotacaoAsync(request, cancellationToken);
            return Ok(relatorio);
        }
    }
}