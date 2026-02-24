using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SintrafGv.Application.DTOs;
using SintrafGv.Application.Interfaces;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Services
{
    public partial class RelatorioService : IRelatorioService
    {
        // Implementação dos relatórios específicos
        
        public async Task<RelatorioResponse<InadimplenciaDto>> ObterRelatorioInadimplenciaAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default)
        {
            // Obter todos os associados
            var (associados, total) = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);

            // TODO: Quando implementar sistema de mensalidades, ajustar esta lógica
            // Por enquanto, vamos simular inadimplência baseado em data de filiação
            var dadosInadimplencia = associados.Select(a => new InadimplenciaDto
            {
                AssociadoId = a.Id,
                Nome = a.Nome,
                Cpf = a.Cpf,
                MatriculaSindicato = a.MatriculaSindicato ?? "",
                MatriculaBancaria = a.MatriculaBancaria ?? "",
                NomeBanco = "Banco Principal", // TODO: Implementar relacionamento com Banco
                MesesAtraso = SimularMesesAtraso(a.DataFiliacao),
                UltimoPagamento = SimularUltimoPagamento(a.DataFiliacao),
                ValorDevido = SimularValorDevido(a.DataFiliacao),
                Telefone = a.Telefone ?? "",
                Celular = a.Celular ?? "",
                Email = a.Email ?? "",
                DataFiliacao = a.DataFiliacao,
                StatusAssociado = a.Ativo ? "Ativo" : "Inativo"
            }).ToList();

            // Aplicar filtros se especificados
            if (request.Filtros?.ContainsKey("mesesAtrasoMinimo") == true)
            {
                if (int.TryParse(request.Filtros["mesesAtrasoMinimo"]?.ToString(), out int minMeses))
                {
                    dadosInadimplencia = dadosInadimplencia.Where(d => d.MesesAtraso >= minMeses).ToList();
                }
            }

            if (request.Filtros?.ContainsKey("apenasAtivos") == true)
            {
                if (bool.TryParse(request.Filtros["apenasAtivos"]?.ToString(), out bool apenasAtivos) && apenasAtivos)
                {
                    dadosInadimplencia = dadosInadimplencia.Where(d => d.StatusAssociado == "Ativo").ToList();
                }
            }

            // Ordenação
            dadosInadimplencia = request.Ordenacao?.Campo switch
            {
                "nome" => request.Ordenacao.Direcao == "desc" 
                    ? dadosInadimplencia.OrderByDescending(x => x.Nome).ToList()
                    : dadosInadimplencia.OrderBy(x => x.Nome).ToList(),
                "mesesAtraso" => request.Ordenacao.Direcao == "desc"
                    ? dadosInadimplencia.OrderByDescending(x => x.MesesAtraso).ToList()
                    : dadosInadimplencia.OrderBy(x => x.MesesAtraso).ToList(),
                "valorDevido" => request.Ordenacao.Direcao == "desc"
                    ? dadosInadimplencia.OrderByDescending(x => x.ValorDevido).ToList()
                    : dadosInadimplencia.OrderBy(x => x.ValorDevido).ToList(),
                _ => dadosInadimplencia.OrderByDescending(x => x.MesesAtraso).ToList()
            };

            // Paginação
            var dadosPaginados = dadosInadimplencia
                .Skip(request.Skip)
                .Take(request.Take)
                .ToList();

            // Totalizadores
            var totalizadores = new Dictionary<string, object>
            {
                { "TotalInadimplentes", dadosInadimplencia.Count },
                { "TotalValorDevido", dadosInadimplencia.Sum(d => d.ValorDevido) },
                { "MediaMesesAtraso", dadosInadimplencia.Any() ? dadosInadimplencia.Average(d => d.MesesAtraso) : 0 },
                { "MaiorDivida", dadosInadimplencia.Any() ? dadosInadimplencia.Max(d => d.ValorDevido) : 0 },
                { "InadimplentesAtivos", dadosInadimplencia.Count(d => d.StatusAssociado == "Ativo") },
                { "InadimplentesInativos", dadosInadimplencia.Count(d => d.StatusAssociado == "Inativo") }
            };

            return new RelatorioResponse<InadimplenciaDto>
            {
                Dados = dadosPaginados,
                Totalizadores = totalizadores,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Inadimplência",
                    Subtitulo = $"Associados com mensalidades em atraso - {DateTime.Now:dd/MM/yyyy}",
                    TipoRelatorio = "inadimplencia",
                    DataGeracao = DateTime.Now,
                    TotalRegistros = dadosInadimplencia.Count,
                    Filtros = request.Filtros ?? new Dictionary<string, object>()
                }
            };
        }

        public async Task<RelatorioResponse<MovimentacaoMensalDto>> ObterRelatorioMovimentacaoMensalAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default)
        {
            // Obter todos os associados
            var (associados, total) = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);

            // Determinar período de análise
            var anoInicio = DateTime.Now.Year - 1;
            var anoFim = DateTime.Now.Year;
            
            if (request.Filtros?.ContainsKey("ano") == true)
            {
                if (int.TryParse(request.Filtros["ano"]?.ToString(), out int ano))
                {
                    anoInicio = ano;
                    anoFim = ano;
                }
            }

            var dadosMovimentacao = new List<MovimentacaoMensalDto>();

            for (int ano = anoInicio; ano <= anoFim; ano++)
            {
                for (int mes = 1; mes <= 12; mes++)
                {
                    var inicioMes = new DateTime(ano, mes, 1);
                    var fimMes = inicioMes.AddMonths(1).AddDays(-1);

                    // Se é mês futuro, pular
                    if (inicioMes > DateTime.Now) continue;

                    // Novas filiações no mês
                    var novasFiliacao = associados.Where(a => 
                        a.DataFiliacao >= inicioMes && a.DataFiliacao <= fimMes).ToList();

                    // Desligamentos no mês
                    var desligamentos = associados.Where(a => 
                        a.DataDesligamento.HasValue && 
                        a.DataDesligamento >= inicioMes && 
                        a.DataDesligamento <= fimMes).ToList();

                    // Total de ativos no final do mês
                    var ativosFinalizados = associados.Count(a => 
                        a.DataFiliacao <= fimMes && 
                        (!a.DataDesligamento.HasValue || a.DataDesligamento > fimMes));

                    var saldoMovimentacao = novasFiliacao.Count - desligamentos.Count;

                    // Calcular percentual de crescimento
                    var ativosInicioMes = associados.Count(a => 
                        a.DataFiliacao < inicioMes && 
                        (!a.DataDesligamento.HasValue || a.DataDesligamento >= inicioMes));

                    var percentualCrescimento = ativosInicioMes > 0 
                        ? (decimal)saldoMovimentacao / ativosInicioMes * 100 
                        : 0;

                    dadosMovimentacao.Add(new MovimentacaoMensalDto
                    {
                        Ano = ano,
                        Mes = mes,
                        MesNome = new DateTime(ano, mes, 1).ToString("MMMM/yyyy"),
                        NovasFiliacao = novasFiliacao.Count,
                        Desligamentos = desligamentos.Count,
                        SaldoMovimentacao = saldoMovimentacao,
                        TotalAtivosFinalizados = ativosFinalizados,
                        PercentualCrescimento = Math.Round(percentualCrescimento, 2),
                        NovosFiliados = novasFiliacao.Select(a => new NovoFiliadoDto
                        {
                            Nome = a.Nome,
                            Cpf = a.Cpf,
                            DataFiliacao = a.DataFiliacao,
                            NomeBanco = "Banco Principal", // TODO: implementar relacionamento
                            Funcao = a.Funcao ?? ""
                        }).ToList(),
                        DetalhesDesligamentos = desligamentos.Select(a => new DesligamentoDto
                        {
                            Nome = a.Nome,
                            Cpf = a.Cpf,
                            DataDesligamento = a.DataDesligamento!.Value,
                            MotivoDesligamento = "Não informado", // TODO: implementar campo motivo
                            NomeBanco = "Banco Principal",
                            TempoFiliacao = a.DataDesligamento!.Value - a.DataFiliacao
                        }).ToList()
                    });
                }
            }

            // Aplicar filtros e ordenação
            dadosMovimentacao = dadosMovimentacao
                .Where(d => d.NovasFiliacao > 0 || d.Desligamentos > 0 || 
                           request.Filtros?.ContainsKey("incluirZeros") == true)
                .OrderBy(d => d.Ano).ThenBy(d => d.Mes)
                .ToList();

            // Totalizadores
            var totalizadores = new Dictionary<string, object>
            {
                { "TotalNovasFiliacao", dadosMovimentacao.Sum(d => d.NovasFiliacao) },
                { "TotalDesligamentos", dadosMovimentacao.Sum(d => d.Desligamentos) },
                { "SaldoGeralMovimentacao", dadosMovimentacao.Sum(d => d.SaldoMovimentacao) },
                { "MediaMensalFiliacao", dadosMovimentacao.Any() ? dadosMovimentacao.Average(d => d.NovasFiliacao) : 0 },
                { "MelhorMes", dadosMovimentacao.OrderByDescending(d => d.SaldoMovimentacao).FirstOrDefault()?.MesNome ?? "" },
                { "CrescimentoAcumulado", dadosMovimentacao.Sum(d => d.PercentualCrescimento) }
            };

            return new RelatorioResponse<MovimentacaoMensalDto>
            {
                Dados = dadosMovimentacao.Skip(request.Skip).Take(request.Take).ToList(),
                Totalizadores = totalizadores,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Movimentação Mensal",
                    Subtitulo = $"Entradas e saídas de associados - {DateTime.Now:dd/MM/yyyy}",
                    TipoRelatorio = "movimentacao-mensal",
                    DataGeracao = DateTime.Now,
                    TotalRegistros = dadosMovimentacao.Count,
                    Filtros = request.Filtros ?? new Dictionary<string, object>()
                }
            };
        }

        #region Métodos auxiliares para simulação (remover quando implementar sistemas reais)

        private int SimularMesesAtraso(DateTime dataFiliacao)
        {
            // Simula inadimplência baseada no tempo de filiação
            var mesesFiliacao = (DateTime.Now - dataFiliacao).Days / 30;
            var random = new Random(dataFiliacao.GetHashCode());
            
            // 30% dos associados têm algum atraso
            if (random.NextDouble() < 0.3)
            {
                return random.Next(1, Math.Min(6, Math.Max(1, mesesFiliacao / 12)));
            }
            return 0;
        }

        private DateTime? SimularUltimoPagamento(DateTime dataFiliacao)
        {
            var mesesAtraso = SimularMesesAtraso(dataFiliacao);
            if (mesesAtraso == 0) return DateTime.Now.AddDays(-15); // Pago recentemente
            
            return DateTime.Now.AddMonths(-mesesAtraso).AddDays(-15);
        }

        private decimal SimularValorDevido(DateTime dataFiliacao)
        {
            var mesesAtraso = SimularMesesAtraso(dataFiliacao);
            var valorMensalidade = 35.50m; // Valor base da mensalidade
            var juros = mesesAtraso * 0.02m; // 2% ao mês
            
            return mesesAtraso * valorMensalidade * (1 + juros);
        }

        #endregion
    }
}