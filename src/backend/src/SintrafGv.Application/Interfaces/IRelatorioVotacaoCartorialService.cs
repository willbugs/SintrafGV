using SintrafGv.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace SintrafGv.Application.Interfaces
{
    public interface IRelatorioVotacaoCartorialService
    {
        /// <summary>
        /// Gera relatório completo de votação para autenticação cartorial
        /// </summary>
        Task<RelatorioVotacaoCartorialDto> GerarRelatorioCartorialAsync(
            RelatorioCartorialRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gera relatório em PDF para cartório
        /// </summary>
        Task<byte[]> GerarRelatorioPDFCartorialAsync(
            RelatorioCartorialRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Valida a integridade dos dados de votação
        /// </summary>
        Task<bool> ValidarIntegridadeVotacaoAsync(
            Guid eleicaoId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gera hash de verificação para o relatório
        /// </summary>
        Task<string> GerarHashVerificacaoAsync(
            RelatorioVotacaoCartorialDto relatorio, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Assina digitalmente o relatório
        /// </summary>
        Task<string> AssinarDigitalmenteRelatorioAsync(
            string hashRelatorio, 
            CancellationToken cancellationToken = default);
    }
}