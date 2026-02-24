using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SintrafGv.Application.DTOs;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Services
{
    public partial class RelatorioService
    {
        public async Task<RelatorioResponse<ParticipacaoVotacaoDto>> ObterRelatorioParticipacaoVotacaoAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default)
        {
            // Obter todos os associados
            var (associados, total) = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);

            // TODO: Quando implementar tabela de relacionamento Voto x Associado, usar dados reais
            // Por enquanto, simular participação baseada em padrões
            var dadosParticipacao = associados.Select(a => {
                var participacao = SimularParticipacaoVotacao(a);
                return new ParticipacaoVotacaoDto
                {
                    AssociadoId = a.Id,
                    Nome = a.Nome,
                    Cpf = a.Cpf,
                    MatriculaSindicato = a.MatriculaSindicato ?? "",
                    NomeBanco = "Banco Principal", // TODO: implementar relacionamento
                    Funcao = a.Funcao ?? "",
                    TotalEleicoesDisponiveis = participacao.totalDisponiveis,
                    TotalVotosRealizados = participacao.totalVotos,
                    PercentualParticipacao = participacao.percentual,
                    UltimaVotacao = participacao.ultimaVotacao,
                    UltimaEleicaoTitulo = participacao.ultimoTitulo,
                    StatusAssociado = a.Ativo ? "Ativo" : "Inativo",
                    DataFiliacao = a.DataFiliacao
                };
            }).ToList();

            // Aplicar filtros
            if (request.Filtros?.ContainsKey("apenasAtivos") == true)
            {
                if (bool.TryParse(request.Filtros["apenasAtivos"]?.ToString(), out bool apenasAtivos) && apenasAtivos)
                {
                    dadosParticipacao = dadosParticipacao.Where(d => d.StatusAssociado == "Ativo").ToList();
                }
            }

            if (request.Filtros?.ContainsKey("participacaoMinima") == true)
            {
                if (decimal.TryParse(request.Filtros["participacaoMinima"]?.ToString(), out decimal minParticipacao))
                {
                    dadosParticipacao = dadosParticipacao.Where(d => d.PercentualParticipacao >= minParticipacao).ToList();
                }
            }

            // Ordenação
            dadosParticipacao = request.Ordenacao?.Campo switch
            {
                "nome" => request.Ordenacao.Direcao == "desc" 
                    ? dadosParticipacao.OrderByDescending(x => x.Nome).ToList()
                    : dadosParticipacao.OrderBy(x => x.Nome).ToList(),
                "participacao" => request.Ordenacao.Direcao == "desc"
                    ? dadosParticipacao.OrderByDescending(x => x.PercentualParticipacao).ToList()
                    : dadosParticipacao.OrderBy(x => x.PercentualParticipacao).ToList(),
                "ultimaVotacao" => request.Ordenacao.Direcao == "desc"
                    ? dadosParticipacao.OrderByDescending(x => x.UltimaVotacao).ToList()
                    : dadosParticipacao.OrderBy(x => x.UltimaVotacao).ToList(),
                _ => dadosParticipacao.OrderByDescending(x => x.PercentualParticipacao).ToList()
            };

            // Totalizadores
            var totalizadores = new Dictionary<string, object>
            {
                { "TotalAssociados", dadosParticipacao.Count },
                { "MediaParticipacao", dadosParticipacao.Any() ? Math.Round(dadosParticipacao.Average(d => d.PercentualParticipacao), 2) : 0 },
                { "AltaParticipacao", dadosParticipacao.Count(d => d.PercentualParticipacao >= 80) },
                { "MediaParticipacao", dadosParticipacao.Count(d => d.PercentualParticipacao >= 50 && d.PercentualParticipacao < 80) },
                { "BaixaParticipacao", dadosParticipacao.Count(d => d.PercentualParticipacao < 50) },
                { "NuncaVotaram", dadosParticipacao.Count(d => d.TotalVotosRealizados == 0) }
            };

            return new RelatorioResponse<ParticipacaoVotacaoDto>
            {
                Dados = dadosParticipacao.Skip(request.Skip).Take(request.Take).ToList(),
                Totalizadores = totalizadores,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Participação em Votações",
                    Subtitulo = $"Análise de engajamento dos associados - {DateTime.Now:dd/MM/yyyy}",
                    TipoRelatorio = "participacao-votacao",
                    DataGeracao = DateTime.Now,
                    TotalRegistros = dadosParticipacao.Count,
                    Filtros = request.Filtros ?? new Dictionary<string, object>()
                }
            };
        }

        public async Task<RelatorioResponse<FaixaEtariaDto>> ObterRelatorioFaixaEtariaAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default)
        {
            // Obter todos os associados
            var (associados, total) = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);

            // Definir faixas etárias
            var faixasEtarias = new[]
            {
                new { Nome = "18-25 anos", Min = 18, Max = 25 },
                new { Nome = "26-35 anos", Min = 26, Max = 35 },
                new { Nome = "36-45 anos", Min = 36, Max = 45 },
                new { Nome = "46-55 anos", Min = 46, Max = 55 },
                new { Nome = "56-65 anos", Min = 56, Max = 65 },
                new { Nome = "Acima de 65 anos", Min = 66, Max = 120 }
            };

            var dadosFaixaEtaria = faixasEtarias.Select(faixa =>
            {
                var associadosFaixa = associados.Where(a =>
                {
                    var idade = CalcularIdade(a.DataNascimento);
                    return idade >= faixa.Min && idade <= faixa.Max;
                }).ToList();

                return new FaixaEtariaDto
                {
                    FaixaEtaria = faixa.Nome,
                    IdadeMinima = faixa.Min,
                    IdadeMaxima = faixa.Max,
                    TotalAssociados = associadosFaixa.Count,
                    AssociadosAtivos = associadosFaixa.Count(a => a.Ativo),
                    AssociadosInativos = associadosFaixa.Count(a => !a.Ativo),
                    PercentualTotal = associados.Any() ? Math.Round((decimal)associadosFaixa.Count / associados.Count() * 100, 2) : 0,
                    IdadeMedia = associadosFaixa.Any() ? Math.Round(associadosFaixa.Average(a => CalcularIdade(a.DataNascimento)), 1) : 0,
                    Detalhes = associadosFaixa.Select(a => new AssociadoFaixaEtariaDto
                    {
                        Nome = a.Nome,
                        Cpf = a.Cpf,
                        DataNascimento = a.DataNascimento,
                        Idade = CalcularIdade(a.DataNascimento),
                        NomeBanco = "Banco Principal", // TODO: implementar relacionamento
                        Funcao = a.Funcao ?? "",
                        Ativo = a.Ativo,
                        DataFiliacao = a.DataFiliacao
                    }).ToList()
                };
            }).Where(f => f.TotalAssociados > 0 || 
                         request.Filtros?.ContainsKey("incluirVazias") == true).ToList();

            // Totalizadores
            var totalizadores = new Dictionary<string, object>
            {
                { "TotalAssociados", associados.Count() },
                { "IdadeMediaGeral", associados.Any() ? Math.Round(associados.Average(a => CalcularIdade(a.DataNascimento)), 1) : 0 },
                { "FaixaMaisNumerosa", dadosFaixaEtaria.OrderByDescending(f => f.TotalAssociados).FirstOrDefault()?.FaixaEtaria ?? "" },
                { "IdadeMaisNova", associados.Any() ? associados.Min(a => CalcularIdade(a.DataNascimento)) : 0 },
                { "IdadeMaisVelha", associados.Any() ? associados.Max(a => CalcularIdade(a.DataNascimento)) : 0 },
                { "PercentualJovens", dadosFaixaEtaria.Where(f => f.IdadeMaxima <= 35).Sum(f => f.PercentualTotal) }
            };

            return new RelatorioResponse<FaixaEtariaDto>
            {
                Dados = dadosFaixaEtaria.Skip(request.Skip).Take(request.Take).ToList(),
                Totalizadores = totalizadores,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Distribuição por Faixa Etária",
                    Subtitulo = $"Demografia dos associados - {DateTime.Now:dd/MM/yyyy}",
                    TipoRelatorio = "faixa-etaria",
                    DataGeracao = DateTime.Now,
                    TotalRegistros = dadosFaixaEtaria.Count,
                    Filtros = request.Filtros ?? new Dictionary<string, object>()
                }
            };
        }

        public async Task<RelatorioResponse<AposentadoPensionistaDto>> ObterRelatorioAposentadosAsync(
            RelatorioRequest request,
            CancellationToken cancellationToken = default)
        {
            // Obter todos os associados
            var (associados, total) = await _associadoRepository.ListarAsync(0, int.MaxValue, false, cancellationToken);

            // Filtrar por aposentados/pensionistas (baseado na idade por enquanto)
            var dadosAposentados = associados.Select(a =>
            {
                var idade = CalcularIdade(a.DataNascimento);
                var tipoBeneficio = DeterminarTipoBeneficio(a, idade);
                
                return new AposentadoPensionistaDto
                {
                    AssociadoId = a.Id,
                    Nome = a.Nome,
                    Cpf = a.Cpf,
                    MatriculaSindicato = a.MatriculaSindicato ?? "",
                    MatriculaBancaria = a.MatriculaBancaria ?? "",
                    NomeBanco = "Banco Principal", // TODO: implementar relacionamento
                    TipoBeneficio = tipoBeneficio,
                    DataAposentadoria = tipoBeneficio != TipoBeneficio.Ativo ? SimularDataAposentadoria(a.DataNascimento, idade) : null,
                    DataPensao = tipoBeneficio == TipoBeneficio.Pensionista ? SimularDataPensao(a.DataNascimento) : null,
                    Ativo = a.Ativo,
                    StatusAssociado = a.Ativo ? "Ativo" : "Inativo",
                    DataFiliacao = a.DataFiliacao,
                    DataDesligamento = a.DataDesligamento,
                    IdadeAtual = idade,
                    TempoContribuicao = a.DataDesligamento?.Subtract(a.DataFiliacao) ?? DateTime.Now.Subtract(a.DataFiliacao),
                    Telefone = a.Telefone ?? "",
                    Email = a.Email ?? ""
                };
            }).Where(a => a.TipoBeneficio != TipoBeneficio.Ativo).ToList();

            // Aplicar filtros
            if (request.Filtros?.ContainsKey("tipoBeneficio") == true)
            {
                if (Enum.TryParse<TipoBeneficio>(request.Filtros["tipoBeneficio"]?.ToString(), out var tipo))
                {
                    dadosAposentados = dadosAposentados.Where(d => d.TipoBeneficio == tipo).ToList();
                }
            }

            if (request.Filtros?.ContainsKey("apenasAtivos") == true)
            {
                if (bool.TryParse(request.Filtros["apenasAtivos"]?.ToString(), out bool apenasAtivos) && apenasAtivos)
                {
                    dadosAposentados = dadosAposentados.Where(d => d.Ativo).ToList();
                }
            }

            // Ordenação
            dadosAposentados = request.Ordenacao?.Campo switch
            {
                "nome" => request.Ordenacao.Direcao == "desc" 
                    ? dadosAposentados.OrderByDescending(x => x.Nome).ToList()
                    : dadosAposentados.OrderBy(x => x.Nome).ToList(),
                "idade" => request.Ordenacao.Direcao == "desc"
                    ? dadosAposentados.OrderByDescending(x => x.IdadeAtual).ToList()
                    : dadosAposentados.OrderBy(x => x.IdadeAtual).ToList(),
                "dataAposentadoria" => request.Ordenacao.Direcao == "desc"
                    ? dadosAposentados.OrderByDescending(x => x.DataAposentadoria).ToList()
                    : dadosAposentados.OrderBy(x => x.DataAposentadoria).ToList(),
                _ => dadosAposentados.OrderBy(x => x.Nome).ToList()
            };

            // Totalizadores
            var totalizadores = new Dictionary<string, object>
            {
                { "TotalAposentadosPensionistas", dadosAposentados.Count },
                { "TotalAposentados", dadosAposentados.Count(d => d.TipoBeneficio == TipoBeneficio.Aposentado) },
                { "TotalPensionistas", dadosAposentados.Count(d => d.TipoBeneficio == TipoBeneficio.Pensionista) },
                { "TotalAposentadoPensionista", dadosAposentados.Count(d => d.TipoBeneficio == TipoBeneficio.AposentadoPensionista) },
                { "IdadeMediaAposentadoria", dadosAposentados.Where(d => d.DataAposentadoria.HasValue).Any() 
                    ? Math.Round(dadosAposentados.Where(d => d.DataAposentadoria.HasValue).Average(d => d.IdadeAtual), 1) : 0 },
                { "AtivosBeneficiarios", dadosAposentados.Count(d => d.Ativo) },
                { "InativosBeneficiarios", dadosAposentados.Count(d => !d.Ativo) }
            };

            return new RelatorioResponse<AposentadoPensionistaDto>
            {
                Dados = dadosAposentados.Skip(request.Skip).Take(request.Take).ToList(),
                Totalizadores = totalizadores,
                Metadata = new RelatorioMetadata
                {
                    Titulo = "Relatório de Aposentados e Pensionistas",
                    Subtitulo = $"Beneficiários do sindicato - {DateTime.Now:dd/MM/yyyy}",
                    TipoRelatorio = "aposentados-pensionistas",
                    DataGeracao = DateTime.Now,
                    TotalRegistros = dadosAposentados.Count,
                    Filtros = request.Filtros ?? new Dictionary<string, object>()
                }
            };
        }

        #region Métodos auxiliares para simulação (complementares)

        private (int totalDisponiveis, int totalVotos, decimal percentual, DateTime? ultimaVotacao, string ultimoTitulo) 
            SimularParticipacaoVotacao(Associado associado)
        {
            var random = new Random(associado.Id.GetHashCode());
            var mesesFiliacao = (DateTime.Now - associado.DataFiliacao).Days / 30;
            var eleicoesDisponiveis = Math.Max(1, mesesFiliacao / 6); // Uma eleição a cada 6 meses aproximadamente
            
            // Simular participação baseada no perfil
            var participacaoBase = associado.Ativo ? 0.7 : 0.3; // Ativos participam mais
            var votosRealizados = 0;
            
            for (int i = 0; i < eleicoesDisponiveis; i++)
            {
                if (random.NextDouble() < participacaoBase)
                {
                    votosRealizados++;
                }
            }

            var percentual = eleicoesDisponiveis > 0 ? (decimal)votosRealizados / eleicoesDisponiveis * 100 : 0;
            var ultimaVotacao = votosRealizados > 0 ? DateTime.Now.AddDays(-random.Next(1, 365)) : (DateTime?)null;
            var ultimoTitulo = ultimaVotacao.HasValue ? $"Eleição Sindical {ultimaVotacao.Value.Year}" : "";

            return (eleicoesDisponiveis, votosRealizados, Math.Round(percentual, 1), ultimaVotacao, ultimoTitulo);
        }

        private int CalcularIdade(DateTime dataNascimento)
        {
            var hoje = DateTime.Today;
            var idade = hoje.Year - dataNascimento.Year;
            if (dataNascimento.Date > hoje.AddYears(-idade)) idade--;
            return Math.Max(0, idade);
        }

        private TipoBeneficio DeterminarTipoBeneficio(Associado associado, int idade)
        {
            // Lógica simplificada - em produção seria baseada em campos específicos
            var random = new Random(associado.Id.GetHashCode());
            
            if (idade >= 65)
            {
                return random.NextDouble() < 0.8 ? TipoBeneficio.Aposentado : TipoBeneficio.AposentadoPensionista;
            }
            else if (idade >= 60)
            {
                return random.NextDouble() < 0.4 ? TipoBeneficio.Aposentado : TipoBeneficio.Ativo;
            }
            else if (!associado.Ativo && random.NextDouble() < 0.2)
            {
                return TipoBeneficio.Pensionista; // Casos especiais
            }
            
            return TipoBeneficio.Ativo;
        }

        private DateTime SimularDataAposentadoria(DateTime dataNascimento, int idade)
        {
            if (idade >= 65)
            {
                return dataNascimento.AddYears(65).AddDays(new Random(dataNascimento.GetHashCode()).Next(-365, 365));
            }
            return dataNascimento.AddYears(60).AddDays(new Random(dataNascimento.GetHashCode()).Next(0, 1825)); // Entre 60-65 anos
        }

        private DateTime SimularDataPensao(DateTime dataNascimento)
        {
            var random = new Random(dataNascimento.GetHashCode());
            return DateTime.Now.AddDays(-random.Next(30, 3650)); // Pensão nos últimos 10 anos
        }

        #endregion
    }
}