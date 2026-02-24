using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SintrafGv.Application.DTOs;
using SintrafGv.Application.Interfaces;

namespace SintrafGv.Application.Services
{
    public partial class RelatorioService : IRelatorioService
    {
        private readonly IAssociadoRepository _associadoRepository;
        private readonly IExportacaoService _exportacaoService;

        public RelatorioService(
            IAssociadoRepository associadoRepository,
            IExportacaoService exportacaoService)
        {
            _associadoRepository = associadoRepository;
            _exportacaoService = exportacaoService;
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioAssociadosGeralAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            
            var dados = associados.Select(MapearAssociadoParaDto).ToList();

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório Geral de Associados",
                    Subtitulo = "Todos os associados",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioAssociadosAtivosAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var ativos = associados.Where(a => a.Ativo).ToList();
            
            var dados = ativos.Select(MapearAssociadoParaDto).ToList();

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Associados Ativos",
                    Subtitulo = "Apenas associados ativos",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioAssociadosInativosAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var inativos = associados.Where(a => !a.Ativo).ToList();
            
            var dados = inativos.Select(MapearAssociadoParaDto).ToList();

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Associados Inativos",
                    Subtitulo = "Apenas associados inativos",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioAniversariantesAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var mes = request.Filtros.ContainsKey("mes") ? 
                Convert.ToInt32(request.Filtros["mes"]) : DateTime.Now.Month;

            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var aniversariantes = associados.Where(a => a.DataNascimento.HasValue && 
                                                      a.DataNascimento.Value.Month == mes).ToList();
            
            var dados = aniversariantes.Select(MapearAssociadoParaDto).ToList();

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Aniversariantes",
                    Subtitulo = $"Mês: {ObterNomeMes(mes)}",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioNovosAssociadosAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var dataInicio = request.Filtros.ContainsKey("dataInicio") ? 
                Convert.ToDateTime(request.Filtros["dataInicio"]) : DateTime.Now.AddMonths(-1);
            var dataFim = request.Filtros.ContainsKey("dataFim") ? 
                Convert.ToDateTime(request.Filtros["dataFim"]) : DateTime.Now;

            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var novos = associados.Where(a => a.DataFiliacao.HasValue && 
                                            a.DataFiliacao >= dataInicio && 
                                            a.DataFiliacao <= dataFim).ToList();
            
            var dados = novos.Select(MapearAssociadoParaDto).ToList();

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Novos Associados",
                    Subtitulo = $"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioPorSexoAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            if (!request.Filtros.ContainsKey("sexo"))
                throw new ArgumentException("Filtro 'sexo' é obrigatório para este relatório");

            var sexo = request.Filtros["sexo"].ToString();
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var porSexo = associados.Where(a => a.Sexo == sexo).ToList();
            
            var dados = porSexo.Select(MapearAssociadoParaDto).ToList();

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório por Sexo",
                    Subtitulo = $"Sexo: {sexo}",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioPorBancoAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var dados = associados.Select(MapearAssociadoParaDto).ToList();

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório por Banco",
                    Subtitulo = "Distribuição por instituição bancária",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioPorCidadeAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var dados = associados.Select(MapearAssociadoParaDto).ToList();

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório por Cidade",
                    Subtitulo = "Distribuição geográfica por cidade",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<DashboardKpiDto> ObterDashboardKpisAsync(CancellationToken cancellationToken = default)
        {
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            var kpis = new DashboardKpiDto
            {
                TotalAssociados = associados.Count(),
                AssociadosAtivos = associados.Count(a => a.Ativo),
                AssociadosInativos = associados.Count(a => !a.Ativo),
                NovosMesAtual = associados.Count(a => a.DataFiliacao >= inicioMes && a.DataFiliacao <= fimMes),
                DesligadosMesAtual = associados.Count(a => a.DataDesligamento >= inicioMes && a.DataDesligamento <= fimMes),
                EnquetesAbertas = 0, // TODO: Implementar quando houver eleições
                EnquetesEncerradas = 0 // TODO: Implementar quando houver eleições
            };

            // Calcular crescimento
            var mesAnterior = inicioMes.AddMonths(-1);
            var fimMesAnterior = inicioMes.AddDays(-1);
            var novosMesAnterior = associados.Count(a => a.DataFiliacao >= mesAnterior && a.DataFiliacao <= fimMesAnterior);
            
            if (novosMesAnterior > 0)
            {
                kpis.PercentualCrescimento = ((decimal)kpis.NovosMesAtual - novosMesAnterior) / novosMesAnterior * 100;
            }

            // Gráfico por sexo
            kpis.GraficoPorSexo = associados
                .Where(a => a.Ativo)
                .GroupBy(a => a.Sexo ?? "Não informado")
                .Select(g => new DashboardGraficoDto
                {
                    Label = g.Key switch
                    {
                        "M" => "Masculino",
                        "F" => "Feminino",
                        _ => "Não informado"
                    },
                    Valor = g.Count(),
                    Cor = g.Key switch
                    {
                        "M" => "#2196F3",
                        "F" => "#E91E63", 
                        _ => "#9E9E9E"
                    }
                })
                .ToList();

            // Gráfico por banco (usando uma propriedade genérica por enquanto)
            kpis.GraficoPorBanco = associados
                .Where(a => a.Ativo)
                .GroupBy(a => "Banco Principal") // Placeholder
                .Select(g => new DashboardGraficoDto
                {
                    Label = g.Key,
                    Valor = g.Count()
                })
                .ToList();

            // Crescimento mensal (últimos 12 meses)
            var crescimentoMensal = new List<DashboardGraficoDto>();
            for (int i = 11; i >= 0; i--)
            {
                var dataRef = hoje.AddMonths(-i);
                var inicioMesRef = new DateTime(dataRef.Year, dataRef.Month, 1);
                var fimMesRef = inicioMesRef.AddMonths(1).AddDays(-1);
                
                var novos = associados.Count(a => a.DataFiliacao >= inicioMesRef && a.DataFiliacao <= fimMesRef);
                
                crescimentoMensal.Add(new DashboardGraficoDto
                {
                    Label = dataRef.ToString("MMM/yyyy"),
                    Valor = novos
                });
            }
            kpis.CrescimentoMensal = crescimentoMensal;

            return kpis;
        }

        // Implementações simplificadas dos outros métodos
        public Task<List<CampoRelatorio>> ObterCamposDisponiveisAsync(string tipoRelatorio, CancellationToken cancellationToken = default)
        {
            var campos = new List<CampoRelatorio>
            {
                new() { Nome = "nome", Titulo = "Nome", Tipo = "string", Filtravel = true, Ordenavel = true },
                new() { Nome = "cpf", Titulo = "CPF", Tipo = "string", Filtravel = true, Ordenavel = true },
                new() { Nome = "ativo", Titulo = "Ativo", Tipo = "boolean", Filtravel = true },
                new() { Nome = "dataNascimento", Titulo = "Data Nascimento", Tipo = "date", Filtravel = true, Ordenavel = true },
                new() { Nome = "dataFiliacao", Titulo = "Data Filiação", Tipo = "date", Filtravel = true, Ordenavel = true },
            };
            
            return Task.FromResult(campos);
        }

        public Task<List<string>> ObterTiposRelatorioAsync(CancellationToken cancellationToken = default)
        {
            var tipos = new List<string>
            {
                "associados-geral",
                "associados-ativos", 
                "associados-inativos",
                "aniversariantes",
                "novos-associados",
                "por-sexo",
                "por-banco",
                "por-cidade"
            };
            
            return Task.FromResult(tipos);
        }

        public async Task<ExportacaoRelatorioDto> ExportarRelatorioAsync(RelatorioRequest request, CancellationToken cancellationToken = default)
        {
            // Obter dados do relatório baseado no tipo
            object dados = request.TipoRelatorio switch
            {
                // Relatórios básicos de associados
                "associados-ativos" => await ObterRelatorioAssociadosAtivosAsync(request, cancellationToken),
                "associados-inativos" => await ObterRelatorioAssociadosInativosAsync(request, cancellationToken),
                "aniversariantes" => await ObterRelatorioAniversariantesAsync(request, cancellationToken),
                "novos-associados" => await ObterRelatorioNovosAssociadosAsync(request, cancellationToken),
                "por-sexo" => await ObterRelatorioPorSexoAsync(request, cancellationToken),
                "por-banco" => await ObterRelatorioPorBancoAsync(request, cancellationToken),
                "por-cidade" => await ObterRelatorioPorCidadeAsync(request, cancellationToken),
                
                // Relatórios específicos de gestão sindical
                "inadimplencia" => await ObterRelatorioInadimplenciaAsync(request, cancellationToken),
                "movimentacao-mensal" => await ObterRelatorioMovimentacaoMensalAsync(request, cancellationToken),
                "participacao-votacao" => await ObterRelatorioParticipacaoVotacaoAsync(request, cancellationToken),
                "faixa-etaria" => await ObterRelatorioFaixaEtariaAsync(request, cancellationToken),
                "aposentados-pensionistas" => await ObterRelatorioAposentadosAsync(request, cancellationToken),
                
                // Padrão
                _ => await ObterRelatorioAssociadosGeralAsync(request, cancellationToken)
            };

            var nomeArquivo = $"{request.TipoRelatorio}_{DateTime.Now:yyyyMMdd_HHmmss}";

            // Como o método de exportação usa generics, precisamos fazer cast dinâmico
            return dados switch
            {
                RelatorioResponse<AssociadoRelatorioDto> r1 => request.FormatoExportacao switch
                {
                    "pdf" => await _exportacaoService.ExportarPdfAsync(r1, nomeArquivo, cancellationToken),
                    "excel" or "xlsx" => await _exportacaoService.ExportarExcelAsync(r1, nomeArquivo, cancellationToken),
                    "csv" => await _exportacaoService.ExportarCsvAsync(r1, nomeArquivo, cancellationToken),
                    _ => throw new ArgumentException($"Formato '{request.FormatoExportacao}' não suportado")
                },
                RelatorioResponse<InadimplenciaDto> r2 => request.FormatoExportacao switch
                {
                    "pdf" => await _exportacaoService.ExportarPdfAsync(r2, nomeArquivo, cancellationToken),
                    "excel" or "xlsx" => await _exportacaoService.ExportarExcelAsync(r2, nomeArquivo, cancellationToken),
                    "csv" => await _exportacaoService.ExportarCsvAsync(r2, nomeArquivo, cancellationToken),
                    _ => throw new ArgumentException($"Formato '{request.FormatoExportacao}' não suportado")
                },
                RelatorioResponse<MovimentacaoMensalDto> r3 => request.FormatoExportacao switch
                {
                    "pdf" => await _exportacaoService.ExportarPdfAsync(r3, nomeArquivo, cancellationToken),
                    "excel" or "xlsx" => await _exportacaoService.ExportarExcelAsync(r3, nomeArquivo, cancellationToken),
                    "csv" => await _exportacaoService.ExportarCsvAsync(r3, nomeArquivo, cancellationToken),
                    _ => throw new ArgumentException($"Formato '{request.FormatoExportacao}' não suportado")
                },
                RelatorioResponse<ParticipacaoVotacaoDto> r4 => request.FormatoExportacao switch
                {
                    "pdf" => await _exportacaoService.ExportarPdfAsync(r4, nomeArquivo, cancellationToken),
                    "excel" or "xlsx" => await _exportacaoService.ExportarExcelAsync(r4, nomeArquivo, cancellationToken),
                    "csv" => await _exportacaoService.ExportarCsvAsync(r4, nomeArquivo, cancellationToken),
                    _ => throw new ArgumentException($"Formato '{request.FormatoExportacao}' não suportado")
                },
                RelatorioResponse<FaixaEtariaDto> r5 => request.FormatoExportacao switch
                {
                    "pdf" => await _exportacaoService.ExportarPdfAsync(r5, nomeArquivo, cancellationToken),
                    "excel" or "xlsx" => await _exportacaoService.ExportarExcelAsync(r5, nomeArquivo, cancellationToken),
                    "csv" => await _exportacaoService.ExportarCsvAsync(r5, nomeArquivo, cancellationToken),
                    _ => throw new ArgumentException($"Formato '{request.FormatoExportacao}' não suportado")
                },
                RelatorioResponse<AposentadoPensionistaDto> r6 => request.FormatoExportacao switch
                {
                    "pdf" => await _exportacaoService.ExportarPdfAsync(r6, nomeArquivo, cancellationToken),
                    "excel" or "xlsx" => await _exportacaoService.ExportarExcelAsync(r6, nomeArquivo, cancellationToken),
                    "csv" => await _exportacaoService.ExportarCsvAsync(r6, nomeArquivo, cancellationToken),
                    _ => throw new ArgumentException($"Formato '{request.FormatoExportacao}' não suportado")
                },
                _ => throw new ArgumentException($"Tipo de relatório '{request.TipoRelatorio}' não suportado para exportação")
            };
        }

        public Task<RelatorioResponse<dynamic>> ExecutarRelatorioCustomizadoAsync(RelatorioRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Funcionalidade será implementada na próxima fase");
        }

        public Task SalvarHistoricoRelatorioAsync(string tipoRelatorio, Guid usuarioId, RelatorioRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<List<dynamic>> ObterHistoricoRelatoriosUsuarioAsync(Guid usuarioId, int limite = 10, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<dynamic>());
        }

        #region Métodos Privados

        private AssociadoRelatorioDto MapearAssociadoParaDto(Domain.Entities.Associado associado)
        {
            return new AssociadoRelatorioDto
            {
                Id = associado.Id,
                Nome = associado.Nome,
                Cpf = associado.Cpf,
                MatriculaSindicato = associado.MatriculaSindicato,
                MatriculaBancaria = associado.MatriculaBancaria,
                Sexo = associado.Sexo,
                EstadoCivil = associado.EstadoCivil,
                Endereco = associado.Endereco,
                Bairro = associado.Bairro,
                Cidade = associado.Cidade,
                Estado = associado.Estado,
                Naturalidade = associado.Naturalidade,
                DataNascimento = associado.DataNascimento,
                Funcao = associado.Funcao,
                DataAdmissao = associado.DataAdmissao,
                DataFiliacao = associado.DataFiliacao,
                DataDesligamento = associado.DataDesligamento,
                Celular = associado.Celular,
                Telefone = associado.Telefone,
                Email = associado.Email,
                NomeBanco = "Banco Principal", // TODO: Implementar campo quando necessário
                Agencia = associado.Agencia,
                Conta = associado.Conta,
                Ativo = associado.Ativo,
                Associado = associado.Ativo, // Simplificação - usando Ativo como proxy
                Aposentado = false, // TODO: Implementar campo se necessário
                Motivo = null, // TODO: Implementar campo se necessário
                Idade = associado.DataNascimento?.CalcularIdade(),
                TempoServico = associado.DataAdmissao?.CalcularTempoServico(),
                TempoFiliacao = associado.DataFiliacao?.CalcularTempoFiliacao()
            };
        }

        private Dictionary<string, object> CalcularTotalizadores(List<AssociadoRelatorioDto> dados)
        {
            return new Dictionary<string, object>
            {
                ["total"] = dados.Count,
                ["ativos"] = dados.Count(d => d.Ativo),
                ["inativos"] = dados.Count(d => !d.Ativo),
                ["masculino"] = dados.Count(d => d.Sexo == "M"),
                ["feminino"] = dados.Count(d => d.Sexo == "F"),
                ["idadeMedia"] = dados.Where(d => d.Idade.HasValue).DefaultIfEmpty().Average(d => d?.Idade ?? 0)
            };
        }

        private string ObterNomeMes(int mes)
        {
            return mes switch
            {
                1 => "Janeiro", 2 => "Fevereiro", 3 => "Março", 4 => "Abril",
                5 => "Maio", 6 => "Junho", 7 => "Julho", 8 => "Agosto",
                9 => "Setembro", 10 => "Outubro", 11 => "Novembro", 12 => "Dezembro",
                _ => "Mês inválido"
            };
        }

        #endregion
    }
}

// Extensions para cálculos
public static class DateTimeExtensions
{
    public static int CalcularIdade(this DateTime dataNascimento)
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - dataNascimento.Year;
        if (dataNascimento.Date > hoje.AddYears(-idade)) idade--;
        return idade;
    }

    public static int CalcularTempoServico(this DateTime dataAdmissao)
    {
        var hoje = DateTime.Today;
        var anos = hoje.Year - dataAdmissao.Year;
        if (dataAdmissao.Date > hoje.AddYears(-anos)) anos--;
        return anos;
    }

    public static int CalcularTempoFiliacao(this DateTime dataFiliacao)
    {
        var hoje = DateTime.Today;
        var anos = hoje.Year - dataFiliacao.Year;
        if (dataFiliacao.Date > hoje.AddYears(-anos)) anos--;
        return anos;
    }
}