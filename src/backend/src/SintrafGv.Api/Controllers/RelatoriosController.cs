using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SintrafGv.Application.DTOs;
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

        /// <summary>
        /// Obtém histórico de relatórios do usuário
        /// </summary>
        [HttpGet("historico")]
        public async Task<ActionResult<dynamic[]>> ObterHistorico(
            int limite = 10, 
            CancellationToken cancellationToken = default)
        {
            // TODO: Obter ID do usuário do JWT
            var usuarioId = Guid.NewGuid(); // Placeholder
            
            var historico = await _relatorioService.ObterHistoricoRelatoriosUsuarioAsync(usuarioId, limite, cancellationToken);
            return Ok(historico);
        }

        /// <summary>
        /// Exporta relatório em formato específico
        /// </summary>
        [HttpPost("exportar")]
        public async Task<IActionResult> ExportarRelatorio([FromBody] RelatorioRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var arquivo = await _relatorioService.ExportarRelatorioAsync(request, cancellationToken);
                
                return File(arquivo.Conteudo, arquivo.ContentType, arquivo.NomeArquivo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao gerar relatório: {ex.Message}");
            }
        }

        // === RELATÓRIOS ESPECÍFICOS DE GESTÃO SINDICAL ===

        /// <summary>
        /// Relatório de Inadimplência - associados com mensalidades em atraso
        /// </summary>
        [HttpPost("inadimplencia")]
        public async Task<ActionResult<RelatorioResponse<InadimplenciaDto>>> RelatorioInadimplencia(
            [FromBody] RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var relatorio = await _relatorioService.ObterRelatorioInadimplenciaAsync(request, cancellationToken);
            return Ok(relatorio);
        }

        /// <summary>
        /// Relatório de Movimentação Mensal - entradas e saídas de associados
        /// </summary>
        [HttpPost("movimentacao-mensal")]
        public async Task<ActionResult<RelatorioResponse<MovimentacaoMensalDto>>> RelatorioMovimentacaoMensal(
            [FromBody] RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var relatorio = await _relatorioService.ObterRelatorioMovimentacaoMensalAsync(request, cancellationToken);
            return Ok(relatorio);
        }

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
        /// Relatório de Distribuição por Faixa Etária - demografia dos associados
        /// </summary>
        [HttpPost("faixa-etaria")]
        public async Task<ActionResult<RelatorioResponse<FaixaEtariaDto>>> RelatorioFaixaEtaria(
            [FromBody] RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var relatorio = await _relatorioService.ObterRelatorioFaixaEtariaAsync(request, cancellationToken);
            return Ok(relatorio);
        }

        /// <summary>
        /// Relatório de Aposentados e Pensionistas - beneficiários do sindicato
        /// </summary>
        [HttpPost("aposentados-pensionistas")]
        public async Task<ActionResult<RelatorioResponse<AposentadoPensionistaDto>>> RelatorioAposentadosPensionistas(
            [FromBody] RelatorioRequest request, 
            CancellationToken cancellationToken)
        {
            var relatorio = await _relatorioService.ObterRelatorioAposentadosAsync(request, cancellationToken);
            return Ok(relatorio);
        }
    }
}