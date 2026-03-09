using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SintrafGv.Application.DTOs;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Application.Interfaces;
using SintrafGv.Infrastructure.Data;

namespace SintrafGv.Application.Services
{
    public partial class RelatorioService : IRelatorioService
    {
        private readonly IAssociadoRepository _associadoRepository;
        private readonly IExportacaoService _exportacaoService;
        private readonly IEleicaoRepository _eleicaoRepository;
        private readonly IVotoRepository _votoRepository;
        private readonly AppDbContext _context;

        public RelatorioService(
            IAssociadoRepository associadoRepository,
            IExportacaoService exportacaoService,
            IEleicaoRepository eleicaoRepository,
            IVotoRepository votoRepository,
            AppDbContext context)
        {
            _associadoRepository = associadoRepository;
            _exportacaoService = exportacaoService;
            _eleicaoRepository = eleicaoRepository;
            _votoRepository = votoRepository;
            _context = context;
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioAssociadosGeralAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var filtros = request.Filtros ?? new Dictionary<string, object>();
            var dados = associados.Select(MapearAssociadoParaDto).ToList();
            dados = AplicarFiltrosAssociados(dados, filtros);
            var (paginaAtual, totalPaginas, paginados) = AplicarPaginacao(dados, request.Pagina, request.TamanhoPagina);

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = paginados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório Geral de Associados",
                    Subtitulo = "Todos os associados",
                    TotalRegistros = dados.Count,
                    PaginaAtual = paginaAtual,
                    TotalPaginas = totalPaginas,
                    FiltrosAplicados = filtros
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
            dados = AplicarFiltrosAssociados(dados, request.Filtros ?? new Dictionary<string, object>());
            var (paginaAtual, totalPaginas, paginados) = AplicarPaginacao(dados, request.Pagina, request.TamanhoPagina);

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = paginados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Associados Ativos",
                    Subtitulo = "Apenas associados ativos",
                    TotalRegistros = dados.Count,
                    PaginaAtual = paginaAtual,
                    TotalPaginas = totalPaginas,
                    FiltrosAplicados = request.Filtros ?? new Dictionary<string, object>()
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
            dados = AplicarFiltrosAssociados(dados, request.Filtros ?? new Dictionary<string, object>());
            var (paginaAtual, totalPaginas, paginados) = AplicarPaginacao(dados, request.Pagina, request.TamanhoPagina);

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = paginados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Associados Inativos",
                    Subtitulo = "Apenas associados inativos",
                    TotalRegistros = dados.Count,
                    PaginaAtual = paginaAtual,
                    TotalPaginas = totalPaginas,
                    FiltrosAplicados = request.Filtros ?? new Dictionary<string, object>()
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioAniversariantesAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var filtros = request.Filtros ?? new Dictionary<string, object>();
            var mes = LerIntFiltro(filtros, "mes") ?? DateTime.Now.Month;
            var dia = LerIntFiltro(filtros, "dia");

            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var aniversariantes = associados.Where(a => a.DataNascimento.HasValue &&
                                                      a.DataNascimento.Value.Month == mes &&
                                                      (!dia.HasValue || a.DataNascimento.Value.Day == dia.Value)).ToList();
            
            var dados = aniversariantes.Select(MapearAssociadoParaDto).ToList();
            dados = AplicarFiltrosAssociados(dados, filtros);
            var (paginaAtual, totalPaginas, paginados) = AplicarPaginacao(dados, request.Pagina, request.TamanhoPagina);

            var subtitulo = dia.HasValue
                ? $"Mês: {ObterNomeMes(mes)}, Dia: {dia.Value}"
                : $"Mês: {ObterNomeMes(mes)}";

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = paginados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Aniversariantes",
                    Subtitulo = subtitulo,
                    TotalRegistros = dados.Count,
                    PaginaAtual = paginaAtual,
                    TotalPaginas = totalPaginas,
                    FiltrosAplicados = filtros
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioNovosAssociadosAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var filtros = request.Filtros ?? new Dictionary<string, object>();
            var dataInicio = LerDataFiltro(filtros, "dataInicio") ?? DateTime.Now.AddMonths(-1);
            var dataFim = LerDataFiltro(filtros, "dataFim") ?? DateTime.Now;

            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var novos = associados.Where(a => a.DataFiliacao.HasValue && 
                                            a.DataFiliacao >= dataInicio && 
                                            a.DataFiliacao <= dataFim).ToList();
            
            var dados = novos.Select(MapearAssociadoParaDto).ToList();
            dados = AplicarFiltrosAssociados(dados, filtros);
            var (paginaAtual, totalPaginas, paginados) = AplicarPaginacao(dados, request.Pagina, request.TamanhoPagina);

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = paginados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Novos Associados",
                    Subtitulo = $"Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}",
                    TotalRegistros = dados.Count,
                    PaginaAtual = paginaAtual,
                    TotalPaginas = totalPaginas,
                    FiltrosAplicados = filtros
                },
                Totalizadores = CalcularTotalizadores(dados)
            };
        }

        public async Task<RelatorioResponse<AssociadoRelatorioDto>> ObterRelatorioPorSexoAsync(
            RelatorioRequest request, 
            CancellationToken cancellationToken = default)
        {
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var filtros = request.Filtros ?? new Dictionary<string, object>();
            var sexo = filtros.TryGetValue("sexo", out var v) && v != null ? v.ToString()?.Trim() : null;
            var porSexo = string.IsNullOrEmpty(sexo)
                ? associados.ToList()
                : associados.Where(a => string.Equals(a.Sexo, sexo, StringComparison.OrdinalIgnoreCase)).ToList();
            var dados = porSexo.Select(MapearAssociadoParaDto).ToList();
            dados = AplicarFiltrosAssociados(dados, request.Filtros ?? new Dictionary<string, object>());
            var (paginaAtual, totalPaginas, paginados) = AplicarPaginacao(dados, request.Pagina, request.TamanhoPagina);

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = paginados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório por Sexo",
                    Subtitulo = string.IsNullOrEmpty(sexo) ? "Todos" : $"Sexo: {sexo}",
                    TotalRegistros = dados.Count,
                    PaginaAtual = paginaAtual,
                    TotalPaginas = totalPaginas,
                    FiltrosAplicados = request.Filtros ?? new Dictionary<string, object>()
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
            dados = AplicarFiltrosAssociados(dados, request.Filtros ?? new Dictionary<string, object>());
            var (paginaAtual, totalPaginas, paginados) = AplicarPaginacao(dados, request.Pagina, request.TamanhoPagina);

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = paginados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório por Banco",
                    Subtitulo = "Distribuição por instituição bancária",
                    TotalRegistros = dados.Count,
                    PaginaAtual = paginaAtual,
                    TotalPaginas = totalPaginas,
                    FiltrosAplicados = request.Filtros ?? new Dictionary<string, object>()
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
            dados = AplicarFiltrosAssociados(dados, request.Filtros ?? new Dictionary<string, object>());
            var (paginaAtual, totalPaginas, paginados) = AplicarPaginacao(dados, request.Pagina, request.TamanhoPagina);

            return new RelatorioResponse<AssociadoRelatorioDto>
            {
                Dados = paginados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório por Cidade",
                    Subtitulo = "Distribuição geográfica por cidade",
                    TotalRegistros = dados.Count,
                    PaginaAtual = paginaAtual,
                    TotalPaginas = totalPaginas,
                    FiltrosAplicados = request.Filtros ?? new Dictionary<string, object>()
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
                EnquetesAbertas = await _eleicaoRepository.ContarAsync(Domain.Entities.StatusEleicao.Aberta, cancellationToken),
                EnquetesEncerradas = await _eleicaoRepository.ContarAsync(Domain.Entities.StatusEleicao.Encerrada, cancellationToken)
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

            kpis.GraficoPorBanco = associados
                .Where(a => a.Ativo)
                .GroupBy(a => a.Banco ?? "Não informado")
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

        public Task<List<CampoRelatorio>> ObterCamposDisponiveisAsync(string tipoRelatorio, CancellationToken cancellationToken = default)
        {
            var campos = new List<CampoRelatorio>
            {
                new() { Nome = "nome", Titulo = "Nome", Tipo = "string", Filtravel = true, Ordenavel = true },
                new() { Nome = "cpf", Titulo = "CPF", Tipo = "string", Filtravel = true, Ordenavel = true },
                new() { Nome = "ativo", Titulo = "Ativo", Tipo = "boolean", Filtravel = true },
                new() { Nome = "sexo", Titulo = "Sexo", Tipo = "string", Filtravel = true, Ordenavel = true },
                new() { Nome = "cidade", Titulo = "Cidade", Tipo = "string", Filtravel = true, Ordenavel = true },
                new() { Nome = "estado", Titulo = "Estado", Tipo = "string", Filtravel = true, Ordenavel = true },
                new() { Nome = "bairro", Titulo = "Bairro", Tipo = "string", Filtravel = true, Ordenavel = true },
                new() { Nome = "nomeBanco", Titulo = "Banco", Tipo = "string", Filtravel = true, Ordenavel = true },
                new() { Nome = "dataNascimento", Titulo = "Data Nascimento", Tipo = "date", Filtravel = true, Ordenavel = true },
                new() { Nome = "dataFiliacao", Titulo = "Data Filiação", Tipo = "date", Filtravel = true, Ordenavel = true },
                new() { Nome = "dataAdmissao", Titulo = "Data Admissão", Tipo = "date", Filtravel = true, Ordenavel = true },
                new() { Nome = "email", Titulo = "E-mail", Tipo = "string", Filtravel = true, Ordenavel = true },
                new() { Nome = "celular", Titulo = "Celular", Tipo = "string", Filtravel = true, Ordenavel = true },
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

        public Task<List<string>> ObterCidadesParaFiltroAsync(CancellationToken cancellationToken = default)
            => _associadoRepository.ObterCidadesDistintasAsync(cancellationToken);

        public Task<List<string>> ObterBancosParaFiltroAsync(CancellationToken cancellationToken = default)
            => _associadoRepository.ObterBancosDistintosAsync(cancellationToken);

        public async Task<ExportacaoRelatorioDto> ExportarRelatorioAsync(RelatorioRequest request, CancellationToken cancellationToken = default)
        {
            // Exportação deve trazer todos os registros filtrados (uma única página grande)
            request.Pagina = 1;
            request.TamanhoPagina = 10000;

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
                
                // Relatórios de votação implementados com novos endpoints específicos
                "participacao-votacao" => await ObterRelatorioParticipacaoVotacaoAsync(request, cancellationToken),
                "resultados-eleicao" => await ObterRelatorioResultadosEleicaoAsync(request, cancellationToken),
                "engajamento-votacao" => await ObterRelatorioEngajamentoVotacaoAsync(request, cancellationToken),
                
                
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
                RelatorioResponse<ResultadoEleicaoDto> r7 => request.FormatoExportacao switch
                {
                    "pdf" => await _exportacaoService.ExportarPdfAsync(r7, nomeArquivo, cancellationToken),
                    "excel" or "xlsx" => await _exportacaoService.ExportarExcelAsync(r7, nomeArquivo, cancellationToken),
                    "csv" => await _exportacaoService.ExportarCsvAsync(r7, nomeArquivo, cancellationToken),
                    _ => throw new ArgumentException($"Formato '{request.FormatoExportacao}' não suportado")
                },
                RelatorioResponse<EngajamentoVotacaoDto> r8 => request.FormatoExportacao switch
                {
                    "pdf" => await _exportacaoService.ExportarPdfAsync(r8, nomeArquivo, cancellationToken),
                    "excel" or "xlsx" => await _exportacaoService.ExportarExcelAsync(r8, nomeArquivo, cancellationToken),
                    "csv" => await _exportacaoService.ExportarCsvAsync(r8, nomeArquivo, cancellationToken),
                    _ => throw new ArgumentException($"Formato '{request.FormatoExportacao}' não suportado")
                },
                _ => throw new ArgumentException($"Tipo de relatório '{request.TipoRelatorio}' não suportado para exportação")
            };
        }

        public async Task<RelatorioResponse<dynamic>> ExecutarRelatorioCustomizadoAsync(RelatorioRequest request, CancellationToken cancellationToken = default)
        {
            var resultado = await ObterRelatorioAssociadosGeralAsync(request, cancellationToken);
            return new RelatorioResponse<dynamic>
            {
                Dados = resultado.Dados.Cast<dynamic>().ToList(),
                Metadata = resultado.Metadata,
                Totalizadores = resultado.Totalizadores
            };
        }

        public async Task<RelatorioResponse<ParticipacaoVotacaoDto>> ObterRelatorioParticipacaoVotacaoAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default)
        {
            // Buscar associados e eleições
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var eleicoes = await _eleicaoRepository.ListarAsync(0, int.MaxValue, null, cancellationToken);
            var votos = await _votoRepository.ListarTodosAsync(cancellationToken);

            var dados = associados.Select(associado =>
            {
                var votosAssociado = votos.Where(v => v.AssociadoId == associado.Id).ToList();
                var eleicoesDisponiveis = eleicoes.Count(e => e.InicioVotacao <= DateTime.Now);
                
                return new ParticipacaoVotacaoDto
                {
                    AssociadoId = associado.Id,
                    Nome = associado.Nome,
                    Cpf = associado.Cpf,
                    MatriculaSindicato = associado.MatriculaSindicato!,
                    NomeBanco = associado.Banco ?? "",
                    Funcao = associado.Funcao!,
                    TotalEleicoesDisponiveis = eleicoesDisponiveis,
                    TotalVotosRealizados = votosAssociado.Count,
                    PercentualParticipacao = eleicoesDisponiveis > 0 ? 
                        (decimal)votosAssociado.Count / eleicoesDisponiveis * 100 : 0,
                    UltimaVotacao = votosAssociado.OrderByDescending(v => v.DataHoraVoto).FirstOrDefault()?.DataHoraVoto,
                    UltimaEleicaoTitulo = votosAssociado.OrderByDescending(v => v.DataHoraVoto).FirstOrDefault()?.Eleicao?.Titulo ?? "",
                    StatusAssociado = associado.Ativo ? "Ativo" : "Inativo",
                    DataFiliacao = associado.DataFiliacao ?? DateTime.MinValue
                };
            }).ToList();

            // Aplicar filtros se especificados
            if (request.Filtros?.ContainsKey("eleicaoId") == true && 
                Guid.TryParse(request.Filtros["eleicaoId"].ToString(), out var eleicaoId))
            {
                var votosEleicao = votos.Where(v => v.EleicaoId == eleicaoId).Select(v => v.AssociadoId).ToHashSet();
                dados = dados.Where(d => votosEleicao.Contains(d.AssociadoId)).ToList();
            }

            return new RelatorioResponse<ParticipacaoVotacaoDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Participação em Votações",
                    Subtitulo = "Análise de engajamento dos associados",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros!
                },
                Totalizadores = new Dictionary<string, object>
                {
                    { "TotalAssociados", dados.Count },
                    { "ParticipacaoMedia", dados.Any() ? dados.Average(d => d.PercentualParticipacao) : 0 },
                    { "TotalVotos", dados.Sum(d => d.TotalVotosRealizados) }
                }
            };
        }

        public async Task<RelatorioResponse<ResultadoEleicaoDto>> ObterRelatorioResultadosEleicaoAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default)
        {
            var eleicoes = await _eleicaoRepository.ListarAsync(0, int.MaxValue, null, cancellationToken);
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            
            if (request.DataInicio.HasValue)
                eleicoes = eleicoes.Where(e => e.InicioVotacao >= request.DataInicio.Value).ToList();
            if (request.DataFim.HasValue)
                eleicoes = eleicoes.Where(e => e.FimVotacao <= request.DataFim.Value).ToList();

            var dados = new List<ResultadoEleicaoDto>();
            
            foreach (var eleicao in eleicoes)
            {
                var votos = await _votoRepository.ListarPorEleicaoAsync(eleicao.Id, cancellationToken);
                
                // Buscar todas as opções (candidatos) da eleição através das perguntas
                var opcoes = eleicao.Perguntas?.SelectMany(p => p.Opcoes ?? new List<Domain.Entities.Opcao>()).ToList() ?? new List<Domain.Entities.Opcao>();
                
                var resultadoCandidatos = opcoes.Select(opcao => 
                {
                    // Contar votos através de VotoDetalhe
                    var votosNaOpcao = _context.VotosDetalhes.Count(vd => vd.OpcaoId == opcao.Id);
                    
                    return new CandidatoResultadoDto
                    {
                        Id = opcao.Id,
                        Nome = opcao.Texto,
                        NumeroVotos = votosNaOpcao,
                        PercentualVotos = votos.Any() ? (decimal)votosNaOpcao / votos.Count * 100 : 0
                    };
                }).OrderByDescending(c => c.NumeroVotos).ToList();

                var totalElegiveis = associados.Count(a => a.Ativo);

                dados.Add(new ResultadoEleicaoDto
                {
                    Id = eleicao.Id,
                    Titulo = eleicao.Titulo,
                    Descricao = eleicao.Descricao!,
                    DataInicio = eleicao.InicioVotacao,
                    DataFim = eleicao.FimVotacao,
                    Status = eleicao.Status.ToString(),
                    TotalVotos = votos.Count,
                    TotalAssociadosElegiveis = totalElegiveis,
                    PercentualParticipacao = totalElegiveis > 0 ? (decimal)votos.Count / totalElegiveis * 100 : 0,
                    Candidatos = resultadoCandidatos,
                    Vencedor = resultadoCandidatos.FirstOrDefault()?.Nome ?? "Não definido"
                });
            }

            return new RelatorioResponse<ResultadoEleicaoDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Resultados de Eleições",
                    Subtitulo = "Detalhamento de resultados por eleição",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros
                },
                Totalizadores = new Dictionary<string, object>
                {
                    { "TotalEleicoes", dados.Count },
                    { "TotalVotosComputados", dados.Sum(d => d.TotalVotos) },
                    { "ParticipacaoMedia", dados.Any() ? dados.Average(d => d.PercentualParticipacao) : 0 }
                }
            };
        }

        public async Task<RelatorioResponse<EngajamentoVotacaoDto>> ObterRelatorioEngajamentoVotacaoAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default)
        {
            var eleicoes = await _eleicaoRepository.ListarAsync(0, int.MaxValue, null, cancellationToken);
            var associados = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);
            var totalElegiveis = associados.Count(a => a.Ativo);
            
            if (request.DataInicio.HasValue)
                eleicoes = eleicoes.Where(e => e.InicioVotacao >= request.DataInicio.Value).ToList();
            if (request.DataFim.HasValue)
                eleicoes = eleicoes.Where(e => e.FimVotacao <= request.DataFim.Value).ToList();

            var dados = new List<EngajamentoVotacaoDto>();

            foreach (var eleicao in eleicoes)
            {
                var votos = await _votoRepository.ListarPorEleicaoAsync(eleicao.Id, cancellationToken);
                var votosPorDia = votos.GroupBy(v => v.DataHoraVoto.Date)
                                      .ToDictionary(g => g.Key.ToString("dd/MM"), g => g.Count());
                
                var votosPorHorario = votos.GroupBy(v => v.DataHoraVoto.Hour)
                                          .ToDictionary(g => $"{g.Key:00}h", g => g.Count());

                var picoVotacao = votosPorDia.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
                DateTime? dataPico = null;
                if (!string.IsNullOrEmpty(picoVotacao.Key))
                {
                    if (DateTime.TryParseExact(picoVotacao.Key, "dd/MM", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                        dataPico = parsedDate;
                }
                
                var dispositivos = votos
                    .GroupBy(v => ClassificarDispositivo(v.UserAgent))
                    .ToDictionary(g => g.Key, g => g.Count());

                dados.Add(new EngajamentoVotacaoDto
                {
                    EleicaoId = eleicao.Id,
                    TituloEleicao = eleicao.Titulo,
                    DataInicio = eleicao.InicioVotacao,
                    DataFim = eleicao.FimVotacao,
                    TotalAssociadosElegiveis = totalElegiveis,
                    TotalVotosComputados = votos.Count,
                    PercentualParticipacao = totalElegiveis > 0 ? (decimal)votos.Count / totalElegiveis * 100 : 0,
                    VotosPorDia = votosPorDia.Values.Any() ? (int)votosPorDia.Values.Average() : 0,
                    PicoVotacao = dataPico,
                    VotosNoPico = picoVotacao.Value,
                    TempoMedioVotacao = CalcularTempoMedioVotacao(votos),
                    VotosPorDispositivo = dispositivos,
                    VotosPorHorario = votosPorHorario,
                    StatusEleicao = eleicao.Status.ToString()
                });
            }

            return new RelatorioResponse<EngajamentoVotacaoDto>
            {
                Dados = dados,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Engajamento em Votações",
                    Subtitulo = "Métricas de participação por período",
                    TotalRegistros = dados.Count,
                    FiltrosAplicados = request.Filtros
                },
                Totalizadores = new Dictionary<string, object>
                {
                    { "TotalEleicoes", dados.Count },
                    { "EngajamentoMedio", dados.Any() ? dados.Average(d => d.PercentualParticipacao) : 0 },
                    { "TotalVotosAnalisados", dados.Sum(d => d.TotalVotosComputados) }
                }
            };
        }

        #region Métodos Privados

        private string ClassificarDispositivo(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Desconhecido";
            var ua = userAgent.ToLower();
            if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
                return "Mobile";
            return "Desktop";
        }

        private TimeSpan CalcularTempoMedioVotacao(List<Domain.Entities.Voto> votos)
        {
            if (votos.Count < 2) return TimeSpan.Zero;
            var ordenados = votos.OrderBy(v => v.DataHoraVoto).ToList();
            var intervalos = new List<double>();
            for (int i = 1; i < ordenados.Count; i++)
            {
                var diff = (ordenados[i].DataHoraVoto - ordenados[i - 1].DataHoraVoto).TotalSeconds;
                if (diff > 0 && diff < 3600)
                    intervalos.Add(diff);
            }
            return intervalos.Any() ? TimeSpan.FromSeconds(intervalos.Average()) : TimeSpan.Zero;
        }

        private List<AssociadoRelatorioDto> AplicarFiltrosAssociados(
            List<AssociadoRelatorioDto> dados,
            Dictionary<string, object>? filtros)
        {
            if (filtros == null || filtros.Count == 0)
                return dados;

            var filtrosAvancados = ExtrairFiltrosAvancados(filtros);
            if (!filtrosAvancados.Any())
            {
                filtrosAvancados = filtros
                    .Where(kvp => !kvp.Key.StartsWith("__", StringComparison.Ordinal))
                    .Select(kvp => (
                        Campo: kvp.Key,
                        Operador: "eq",
                        Valor: (object?)kvp.Value,
                        ValorAte: (object?)null,
                        Condicao: "and"))
                    .ToList();
            }

            if (!filtrosAvancados.Any())
                return dados;

            return dados.Where(item =>
            {
                bool acumulado = false;
                bool primeiro = true;
                foreach (var (campo, operador, valor, valorAte, condicao) in filtrosAvancados)
                {
                    var resultadoFiltro = AvaliarFiltroAssociado(item, campo, operador, valor, valorAte);
                    if (primeiro)
                    {
                        acumulado = resultadoFiltro;
                        primeiro = false;
                    }
                    else
                    {
                        acumulado = string.Equals(condicao, "or", StringComparison.OrdinalIgnoreCase)
                            ? acumulado || resultadoFiltro
                            : acumulado && resultadoFiltro;
                    }
                }
                return acumulado;
            }).ToList();
        }

        private List<(string Campo, string Operador, object? Valor, object? ValorAte, string Condicao)> ExtrairFiltrosAvancados(
            Dictionary<string, object> filtros)
        {
            if (!filtros.TryGetValue("__filtrosAvancados", out var bruto) || bruto == null)
                return new();

            if (bruto is JsonElement json)
            {
                var resultado = new List<(string, string, object?, object?, string)>();
                if (json.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in json.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object) continue;
                        var campo = item.TryGetProperty("campo", out var c) ? c.GetString() ?? "" : "";
                        if (string.IsNullOrWhiteSpace(campo)) continue;
                        var operador = item.TryGetProperty("operador", out var o) ? o.GetString() ?? "eq" : "eq";
                        var condicao = item.TryGetProperty("condicao", out var cd) ? cd.GetString() ?? "and" : "and";
                        object? valor = item.TryGetProperty("valor", out var v) ? JsonElementParaObjeto(v) : null;
                        object? valorAte = item.TryGetProperty("valorAte", out var va) ? JsonElementParaObjeto(va) : null;
                        resultado.Add((campo, operador, valor, valorAte, condicao));
                    }
                }
                else if (json.ValueKind == JsonValueKind.Object)
                {
                    // fallback: recebe um único filtro como objeto
                    var campo = json.TryGetProperty("campo", out var c) ? c.GetString() ?? "" : "";
                    if (!string.IsNullOrWhiteSpace(campo))
                    {
                        var operador = json.TryGetProperty("operador", out var o) ? o.GetString() ?? "eq" : "eq";
                        var condicao = json.TryGetProperty("condicao", out var cd) ? cd.GetString() ?? "and" : "and";
                        object? valor = json.TryGetProperty("valor", out var v) ? JsonElementParaObjeto(v) : null;
                        object? valorAte = json.TryGetProperty("valorAte", out var va) ? JsonElementParaObjeto(va) : null;
                        resultado.Add((campo, operador, valor, valorAte, condicao));
                    }
                }
                if (resultado.Any()) return resultado;
            }

            if (bruto is IEnumerable enumerable and not string)
            {
                var resultado = new List<(string, string, object?, object?, string)>();
                foreach (var item in enumerable)
                {
                    if (item == null) continue;
                    if (TryLerFiltroObj(item, out var filtro))
                        resultado.Add(filtro);
                }
                if (resultado.Any()) return resultado;
            }

            return new();
        }

        private static bool TryLerFiltroObj(
            object item,
            out (string Campo, string Operador, object? Valor, object? ValorAte, string Condicao) filtro)
        {
            filtro = ("", "eq", null, null, "and");

            if (item is JsonElement je && je.ValueKind == JsonValueKind.Object)
            {
                var campo = je.TryGetProperty("campo", out var c) ? c.GetString() ?? "" : "";
                if (string.IsNullOrWhiteSpace(campo)) return false;
                var operador = je.TryGetProperty("operador", out var o) ? o.GetString() ?? "eq" : "eq";
                var condicao = je.TryGetProperty("condicao", out var cd) ? cd.GetString() ?? "and" : "and";
                object? valor = je.TryGetProperty("valor", out var v) ? JsonElementParaObjeto(v) : null;
                object? valorAte = je.TryGetProperty("valorAte", out var va) ? JsonElementParaObjeto(va) : null;
                filtro = (campo, operador, valor, valorAte, condicao);
                return true;
            }

            return false;
        }

        private static object? JsonElementParaObjeto(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number when element.TryGetInt64(out var l) => l,
                JsonValueKind.Number when element.TryGetDecimal(out var d) => d,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => element.ToString()
            };
        }

        private static bool AvaliarFiltroAssociado(
            AssociadoRelatorioDto item,
            string campo,
            string operador,
            object? valor,
            object? valorAte)
        {
            switch (campo)
            {
                case "nome":
                    return AvaliarString(item.Nome, operador, valor);
                case "cpf":
                    return AvaliarString(item.Cpf, operador, valor);
                case "ativo":
                    return AvaliarBool(item.Ativo, operador, valor);
                case "sexo":
                    return AvaliarString(item.Sexo, operador, valor);
                case "cidade":
                    if (operador == "eq" && string.Equals(valor?.ToString(), "Não informado", StringComparison.OrdinalIgnoreCase))
                        return string.IsNullOrWhiteSpace(item.Cidade);
                    return AvaliarString(item.Cidade, operador, valor);
                case "estado":
                    return AvaliarString(item.Estado, operador, valor);
                case "bairro":
                    return AvaliarString(item.Bairro, operador, valor);
                case "nomeBanco":
                    return AvaliarString(item.NomeBanco, operador, valor);
                case "email":
                    return AvaliarString(item.Email, operador, valor);
                case "celular":
                    return AvaliarString(item.Celular, operador, valor);
                case "dataNascimento":
                    return AvaliarData(item.DataNascimento, operador, valor, valorAte);
                case "dataFiliacao":
                    return AvaliarData(item.DataFiliacao, operador, valor, valorAte);
                case "dataAdmissao":
                    return AvaliarData(item.DataAdmissao, operador, valor, valorAte);
                default:
                    return true;
            }
        }

        private static bool AvaliarString(string? atual, string operador, object? valor)
        {
            var a = atual ?? "";
            var b = valor?.ToString() ?? "";
            return operador switch
            {
                "eq" => string.Equals(a, b, StringComparison.OrdinalIgnoreCase),
                "ne" => !string.Equals(a, b, StringComparison.OrdinalIgnoreCase),
                "contains" => a.Contains(b, StringComparison.OrdinalIgnoreCase),
                "startswith" => a.StartsWith(b, StringComparison.OrdinalIgnoreCase),
                "endswith" => a.EndsWith(b, StringComparison.OrdinalIgnoreCase),
                _ => true
            };
        }

        private static bool AvaliarBool(bool atual, string operador, object? valor)
        {
            if (!bool.TryParse(valor?.ToString(), out var esperado))
                return true;
            return operador switch
            {
                "eq" => atual == esperado,
                "ne" => atual != esperado,
                _ => true
            };
        }

        private static bool AvaliarData(DateTime? atual, string operador, object? valor, object? valorAte)
        {
            if (!atual.HasValue) return false;
            if (!DateTime.TryParse(valor?.ToString(), out var inicio))
                return true;
            var dataAtual = atual.Value.Date;
            var dataInicio = inicio.Date;
            var dataFim = dataInicio;
            if (operador == "between" && DateTime.TryParse(valorAte?.ToString(), out var ate))
                dataFim = ate.Date;

            return operador switch
            {
                "eq" => dataAtual == dataInicio,
                "ne" => dataAtual != dataInicio,
                "gt" => dataAtual > dataInicio,
                "lt" => dataAtual < dataInicio,
                "gte" => dataAtual >= dataInicio,
                "lte" => dataAtual <= dataInicio,
                "between" => dataAtual >= dataInicio && dataAtual <= dataFim,
                _ => true
            };
        }

        private static int? LerIntFiltro(Dictionary<string, object> filtros, string chave)
        {
            if (!filtros.TryGetValue(chave, out var bruto) || bruto == null)
                return null;
            if (bruto is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out var n)) return n;
                if (je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out n)) return n;
                return null;
            }
            return int.TryParse(bruto.ToString(), out var valor) ? valor : null;
        }

        private static DateTime? LerDataFiltro(Dictionary<string, object> filtros, string chave)
        {
            if (!filtros.TryGetValue(chave, out var bruto) || bruto == null)
                return null;
            if (bruto is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.String && DateTime.TryParse(je.GetString(), out var d1)) return d1;
                return null;
            }
            return DateTime.TryParse(bruto.ToString(), out var d2) ? d2 : null;
        }

        /// <summary>
        /// Aplica paginação à lista filtrada. Retorna (página atual, total de páginas, lista paginada).
        /// </summary>
        private static (int paginaAtual, int totalPaginas, List<AssociadoRelatorioDto> paginados) AplicarPaginacao(
            List<AssociadoRelatorioDto> dados,
            int? pagina,
            int? tamanhoPagina)
        {
            var total = dados.Count;
            var p = Math.Max(1, pagina ?? 1);
            var t = tamanhoPagina ?? 25;
            if (t < 1) t = 25;
            if (t > 10000) t = 10000; // limite alto para permitir exportação com todos os registros
            var totalPaginas = total == 0 ? 1 : (int)Math.Ceiling(total / (double)t);
            var skip = (p - 1) * t;
            var paginados = dados.Skip(skip).Take(t).ToList();
            return (p, totalPaginas, paginados);
        }

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
                NomeBanco = associado.Banco ?? "Não informado",
                Agencia = associado.Agencia,
                Conta = associado.Conta,
                Ativo = associado.Ativo,
                Associado = associado.Ativo,
                Aposentado = associado.Aposentado,
                Motivo = null,
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