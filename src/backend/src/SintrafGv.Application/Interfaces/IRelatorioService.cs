using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SintrafGv.Application.DTOs;

namespace SintrafGv.Application.Interfaces
{
    public interface IRelatorioService
    {
        // Relatórios de Associados
        Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioAssociadosGeralAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioAssociadosAtivosAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioAssociadosInativosAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioAniversariantesAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioNovosAssociadosAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioPorSexoAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioPorBancoAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioPorCidadeAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default);

        // Dashboard
        Task<DashboardKpiDto> ObterDashboardKpisAsync(CancellationToken cancellationToken = default);

        // Metadata e Configurações
        Task<List<CampoRelatorio>> ObterCamposDisponiveisAsync(
            string tipoRelatorio, 
            CancellationToken cancellationToken = default);

        Task<List<string>> ObterTiposRelatorioAsync(CancellationToken cancellationToken = default);

        // Exportação
        Task<ExportacaoRelatorioDto> ExportarRelatorioAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default);

        // Relatórios Customizados
        Task<RelatorioResponse<dynamic>> ExecutarRelatorioCustomizadoAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default);

        // Histórico e Auditoria
        Task SalvarHistoricoRelatorioAsync(
            string tipoRelatorio, 
            Guid usuarioId, 
            RelatorioRequest request,
            CancellationToken cancellationToken = default);

        Task<List<dynamic>> ObterHistoricoRelatoriosUsuarioAsync(
            Guid usuarioId, 
            int limite = 10,
            CancellationToken cancellationToken = default);

        // Relatórios Específicos de Gestão Sindical
        Task<RelatorioResponse<InadimplenciaDto>> ObterRelatorioInadimplenciaAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<MovimentacaoMensalDto>> ObterRelatorioMovimentacaoMensalAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<ParticipacaoVotacaoDto>> ObterRelatorioParticipacaoVotacaoAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<FaixaEtariaDto>> ObterRelatorioFaixaEtariaAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default);

        Task<RelatorioResponse<AposentadoPensionistaDto>> ObterRelatorioAposentadosAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default);
    }
}