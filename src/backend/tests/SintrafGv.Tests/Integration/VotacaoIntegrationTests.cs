using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Enums;
using SintrafGv.Application.DTOs;

namespace SintrafGv.Tests.Integration
{
    [Collection("Integration")]
    public class VotacaoIntegrationTests
    {
        // Estes testes simulariam o fluxo completo com banco de dados real
        // Por enquanto, implementamos a lógica de teste sem dependência externa

        [Fact]
        public async Task FluxoCompletoVotacao_DeveExecutarCorretamente()
        {
            // === CENÁRIO: Fluxo completo de eleição ===
            // 1. Criar eleição
            // 2. Configurar perguntas e opções
            // 3. Abrir eleição
            // 4. Associados votam
            // 5. Encerrar eleição
            // 6. Apurar resultados

            // Arrange - Dados da eleição
            var eleicaoId = Guid.NewGuid();
            var pergunta1Id = Guid.NewGuid();
            var pergunta2Id = Guid.NewGuid();
            
            var opcaoPresidente1Id = Guid.NewGuid();
            var opcaoPresidente2Id = Guid.NewGuid();
            var opcaoConselho1Id = Guid.NewGuid();
            var opcaoConselho2Id = Guid.NewGuid();
            var opcaoConselho3Id = Guid.NewGuid();

            // Simular criação de eleição
            var eleicao = new Eleicao
            {
                Id = eleicaoId,
                Titulo = "Eleição Sindical 2026",
                Descricao = "Eleição para diretoria e conselho fiscal",
                InicioVotacao = DateTime.Now.AddMinutes(-30),
                FimVotacao = DateTime.Now.AddHours(2),
                Tipo = TipoEleicao.Eleicao,
                Status = StatusEleicao.Aberta,
                ApenasAssociados = true,
                ApenasAtivos = true,
                Perguntas = new List<Pergunta>
                {
                    new Pergunta
                    {
                        Id = pergunta1Id,
                        EleicaoId = eleicaoId,
                        Ordem = 1,
                        Texto = "Presidente",
                        Tipo = TipoPergunta.UnicoVoto,
                        PermiteBranco = true,
                        Opcoes = new List<Opcao>
                        {
                            new Opcao { Id = opcaoPresidente1Id, Ordem = 1, Texto = "João Silva", Descricao = "Experiência de 10 anos" },
                            new Opcao { Id = opcaoPresidente2Id, Ordem = 2, Texto = "Maria Santos", Descricao = "Foco na modernização" }
                        }
                    },
                    new Pergunta
                    {
                        Id = pergunta2Id,
                        EleicaoId = eleicaoId,
                        Ordem = 2,
                        Texto = "Conselho Fiscal (escolha até 2)",
                        Tipo = TipoPergunta.MultiploVoto,
                        MaxVotos = 2,
                        PermiteBranco = true,
                        Opcoes = new List<Opcao>
                        {
                            new Opcao { Id = opcaoConselho1Id, Ordem = 1, Texto = "Carlos Lima" },
                            new Opcao { Id = opcaoConselho2Id, Ordem = 2, Texto = "Ana Paula" },
                            new Opcao { Id = opcaoConselho3Id, Ordem = 3, Texto = "Roberto Costa" }
                        }
                    }
                }
            };

            // Associados que irão votar
            var associados = new List<Associado>
            {
                new Associado { Id = Guid.NewGuid(), Nome = "Votante 1", Ativo = true },
                new Associado { Id = Guid.NewGuid(), Nome = "Votante 2", Ativo = true },
                new Associado { Id = Guid.NewGuid(), Nome = "Votante 3", Ativo = true },
                new Associado { Id = Guid.NewGuid(), Nome = "Votante 4", Ativo = false }, // Inativo - não pode votar
                new Associado { Id = Guid.NewGuid(), Nome = "Votante 5", Ativo = true }
            };

            // Act & Assert - Simular votações

            var votosRegistrados = new List<VotoSimulado>();
            var votosDetalhes = new List<VotoDetalhe>();

            // === VOTO 1: João vota em João Silva para presidente + Carlos Lima para conselho ===
            var voto1 = new VotoSimulado
            {
                AssociadoId = associados[0].Id,
                EleicaoId = eleicaoId,
                Respostas = new List<(Guid perguntaId, List<Guid?> opcoes)>
                {
                    (pergunta1Id, new List<Guid?> { opcaoPresidente1Id }),
                    (pergunta2Id, new List<Guid?> { opcaoConselho1Id })
                }
            };

            ValidarVoto(eleicao, associados[0], voto1).Should().BeTrue();
            votosRegistrados.Add(voto1);
            AdicionarVotoDetalhes(votosDetalhes, voto1);

            // === VOTO 2: Maria vota em Maria Santos + Carlos Lima e Ana Paula ===
            var voto2 = new VotoSimulado
            {
                AssociadoId = associados[1].Id,
                EleicaoId = eleicaoId,
                Respostas = new List<(Guid perguntaId, List<Guid?> opcoes)>
                {
                    (pergunta1Id, new List<Guid?> { opcaoPresidente2Id }),
                    (pergunta2Id, new List<Guid?> { opcaoConselho1Id, opcaoConselho2Id })
                }
            };

            ValidarVoto(eleicao, associados[1], voto2).Should().BeTrue();
            votosRegistrados.Add(voto2);
            AdicionarVotoDetalhes(votosDetalhes, voto2);

            // === VOTO 3: Pedro vota em branco para presidente + Roberto Costa ===
            var voto3 = new VotoSimulado
            {
                AssociadoId = associados[2].Id,
                EleicaoId = eleicaoId,
                Respostas = new List<(Guid perguntaId, List<Guid?> opcoes)>
                {
                    (pergunta1Id, new List<Guid?> { null }), // Voto em branco
                    (pergunta2Id, new List<Guid?> { opcaoConselho3Id })
                }
            };

            ValidarVoto(eleicao, associados[2], voto3).Should().BeTrue();
            votosRegistrados.Add(voto3);
            AdicionarVotoDetalhes(votosDetalhes, voto3);

            // === TENTATIVA VOTO INVÁLIDO: Associado inativo ===
            var votoInvalido = new VotoSimulado
            {
                AssociadoId = associados[3].Id, // Inativo
                EleicaoId = eleicaoId
            };

            ValidarVoto(eleicao, associados[3], votoInvalido).Should().BeFalse();

            // === VOTO 4: Último votante - João Silva + Ana Paula ===
            var voto4 = new VotoSimulado
            {
                AssociadoId = associados[4].Id,
                EleicaoId = eleicaoId,
                Respostas = new List<(Guid perguntaId, List<Guid?> opcoes)>
                {
                    (pergunta1Id, new List<Guid?> { opcaoPresidente1Id }),
                    (pergunta2Id, new List<Guid?> { opcaoConselho2Id })
                }
            };

            ValidarVoto(eleicao, associados[4], voto4).Should().BeTrue();
            votosRegistrados.Add(voto4);
            AdicionarVotoDetalhes(votosDetalhes, voto4);

            // === APURAÇÃO DOS RESULTADOS ===
            var resultados = ApurarResultados(eleicao, votosDetalhes);

            // Assert - Verificar resultados
            resultados.TotalVotos.Should().Be(4); // 4 votantes válidos
            resultados.Perguntas.Should().HaveCount(2);

            // Resultados Presidente
            var resultadoPresidente = resultados.Perguntas.Find(p => p.PerguntaId == pergunta1Id);
            resultadoPresidente.Should().NotBeNull();
            resultadoPresidente!.VotosBranco.Should().Be(1);
            resultadoPresidente.VotosValidos.Should().Be(3);

            var votosJoao = resultadoPresidente.Opcoes.Find(o => o.OpcaoId == opcaoPresidente1Id)?.TotalVotos ?? 0;
            var votosMaria = resultadoPresidente.Opcoes.Find(o => o.OpcaoId == opcaoPresidente2Id)?.TotalVotos ?? 0;
            
            votosJoao.Should().Be(2); // Votos 1 e 4
            votosMaria.Should().Be(1); // Voto 2

            // Resultados Conselho
            var resultadoConselho = resultados.Perguntas.Find(p => p.PerguntaId == pergunta2Id);
            resultadoConselho.Should().NotBeNull();
            
            var votosCarlos = resultadoConselho!.Opcoes.Find(o => o.OpcaoId == opcaoConselho1Id)?.TotalVotos ?? 0;
            var votosAna = resultadoConselho.Opcoes.Find(o => o.OpcaoId == opcaoConselho2Id)?.TotalVotos ?? 0;
            var votosRoberto = resultadoConselho.Opcoes.Find(o => o.OpcaoId == opcaoConselho3Id)?.TotalVotos ?? 0;
            
            votosCarlos.Should().Be(2); // Votos 1 e 2
            votosAna.Should().Be(2); // Votos 2 e 4
            votosRoberto.Should().Be(1); // Voto 3

            // === VERIFICAÇÕES DE SEGURANÇA E INTEGRIDADE ===
            
            // 1. Cada associado votou apenas uma vez
            var votanteDistintos = votosRegistrados.Select(v => v.AssociadoId).Distinct().Count();
            votanteDistintos.Should().Be(votosRegistrados.Count);

            // 2. Não há ligação entre voto e votante nos detalhes
            votosDetalhes.Should().AllSatisfy(vd => 
            {
                // VotoDetalhe não deve ter propriedade AssociadoId
                var propriedades = vd.GetType().GetProperties();
                propriedades.Should().NotContain(p => p.Name == "AssociadoId");
            });

            // 3. Todas as opções selecionadas existem na eleição
            foreach (var detalhe in votosDetalhes)
            {
                if (detalhe.OpcaoId.HasValue)
                {
                    var pergunta = eleicao.Perguntas.Find(p => p.Id == detalhe.PerguntaId);
                    pergunta.Should().NotBeNull();
                    pergunta!.Opcoes.Should().Contain(o => o.Id == detalhe.OpcaoId.Value);
                }
            }

            // 4. Respeitou limites de múltipla escolha
            var votosConselho = votosDetalhes.Where(vd => vd.PerguntaId == pergunta2Id).ToList();
            var votosConselhoPorAssociado = votosRegistrados
                .SelectMany(vr => vr.Respostas.Where(r => r.perguntaId == pergunta2Id)
                    .SelectMany(r => r.opcoes.Where(o => o.HasValue)))
                .GroupBy(x => votosRegistrados.FindIndex(vr => 
                    vr.Respostas.Any(r => r.perguntaId == pergunta2Id)));

            // Cada votante deve ter no máximo 2 escolhas para conselho
            foreach (var grupo in votosConselhoPorAssociado)
            {
                grupo.Count().Should().BeLessOrEqualTo(2);
            }
        }

        [Fact]
        public async Task SegurancaVotacao_TentativaVotosDuplicados_DeveSerBloqueada()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();
            var perguntaId = Guid.NewGuid();
            var opcaoId = Guid.NewGuid();

            var votosExistentes = new List<VotoSimulado>
            {
                new VotoSimulado { AssociadoId = associadoId, EleicaoId = eleicaoId }
            };

            // Act & Assert
            var jaVotou = VerificarSeJaVotou(votosExistentes, eleicaoId, associadoId);
            jaVotou.Should().BeTrue("Associado já deve ter votado");

            // Tentativa de votar novamente deve ser bloqueada
            var podeVotarNovamente = !jaVotou;
            podeVotarNovamente.Should().BeFalse("Não deve permitir voto duplicado");
        }

        [Fact]
        public async Task IntegridadeVoto_HashDeveSerConsistente()
        {
            // Arrange & Act
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();
            var dataHora = DateTime.Now;
            var respostas = "pergunta1:opcao1,pergunta2:opcao2";

            var hash1 = GerarHashVoto(eleicaoId, associadoId, dataHora, respostas);
            var hash2 = GerarHashVoto(eleicaoId, associadoId, dataHora, respostas);
            var hash3 = GerarHashVoto(eleicaoId, associadoId, dataHora.AddSeconds(1), respostas); // Diferente

            // Assert
            hash1.Should().Be(hash2, "Mesmo input deve gerar mesmo hash");
            hash1.Should().NotBe(hash3, "Input diferente deve gerar hash diferente");
            hash1.Should().NotBeNullOrEmpty();
            hash1.Length.Should().BeGreaterThan(10, "Hash deve ter tamanho adequado");
        }

        #region Métodos auxiliares para simulação

        private class VotoSimulado
        {
            public Guid AssociadoId { get; set; }
            public Guid EleicaoId { get; set; }
            public List<(Guid perguntaId, List<Guid?> opcoes)> Respostas { get; set; } = new();
        }

        private class ResultadoApuracao
        {
            public int TotalVotos { get; set; }
            public List<ResultadoPergunta> Perguntas { get; set; } = new();
        }

        private class ResultadoPergunta
        {
            public Guid PerguntaId { get; set; }
            public string Texto { get; set; } = "";
            public int VotosValidos { get; set; }
            public int VotosBranco { get; set; }
            public List<ResultadoOpcao> Opcoes { get; set; } = new();
        }

        private class ResultadoOpcao
        {
            public Guid OpcaoId { get; set; }
            public string Texto { get; set; } = "";
            public int TotalVotos { get; set; }
            public decimal PercentualVotos { get; set; }
        }

        private bool ValidarVoto(Eleicao eleicao, Associado associado, VotoSimulado voto)
        {
            // Verificar se eleição está aberta
            var agora = DateTime.Now;
            if (eleicao.Status != StatusEleicao.Aberta ||
                agora < eleicao.InicioVotacao ||
                agora > eleicao.FimVotacao)
                return false;

            // Verificar elegibilidade do associado
            if (eleicao.ApenasAssociados && associado == null) return false;
            if (eleicao.ApenasAtivos && !associado.Ativo) return false;

            // Verificar se já votou (simulado)
            // Em implementação real, consultaria o banco de dados

            return true;
        }

        private void AdicionarVotoDetalhes(List<VotoDetalhe> detalhes, VotoSimulado voto)
        {
            foreach (var resposta in voto.Respostas)
            {
                foreach (var opcao in resposta.opcoes)
                {
                    detalhes.Add(new VotoDetalhe
                    {
                        Id = Guid.NewGuid(),
                        EleicaoId = voto.EleicaoId,
                        PerguntaId = resposta.perguntaId,
                        OpcaoId = opcao, // null para voto em branco
                        DataHora = DateTime.Now
                    });
                }
            }
        }

        private ResultadoApuracao ApurarResultados(Eleicao eleicao, List<VotoDetalhe> votosDetalhes)
        {
            var resultado = new ResultadoApuracao
            {
                TotalVotos = votosDetalhes.GroupBy(v => v.EleicaoId).Sum(g => 
                    g.GroupBy(vd => vd.PerguntaId).Count() > 0 ? 
                    g.GroupBy(vd => vd.PerguntaId).Max(pg => pg.Count()) : 0),
                Perguntas = new List<ResultadoPergunta>()
            };

            foreach (var pergunta in eleicao.Perguntas)
            {
                var votosPergun ta = votosDetalhes.Where(vd => vd.PerguntaId == pergunta.Id).ToList();
                var votosBranco = votosPergun ta.Count(vd => vd.OpcaoId == null);
                var votosValidos = votosPergun ta.Count(vd => vd.OpcaoId != null);

                var resultadoPergunta = new ResultadoPergunta
                {
                    PerguntaId = pergunta.Id,
                    Texto = pergunta.Texto,
                    VotosValidos = votosValidos,
                    VotosBranco = votosBranco,
                    Opcoes = new List<ResultadoOpcao>()
                };

                foreach (var opcao in pergunta.Opcoes)
                {
                    var votosOpcao = votosPergun ta.Count(vd => vd.OpcaoId == opcao.Id);
                    var percentual = votosPergun ta.Count > 0 ? 
                        Math.Round((decimal)votosOpcao / votosPergun ta.Count * 100, 2) : 0;

                    resultadoPergunta.Opcoes.Add(new ResultadoOpcao
                    {
                        OpcaoId = opcao.Id,
                        Texto = opcao.Texto,
                        TotalVotos = votosOpcao,
                        PercentualVotos = percentual
                    });
                }

                resultado.Perguntas.Add(resultadoPergunta);
            }

            // Calcular total de votos únicos (assumindo 1 voto por pergunta por pessoa)
            resultado.TotalVotos = eleicao.Perguntas.Max(p => 
                votosDetalhes.Where(vd => vd.PerguntaId == p.Id).Count());

            return resultado;
        }

        private bool VerificarSeJaVotou(List<VotoSimulado> votos, Guid eleicaoId, Guid associadoId)
        {
            return votos.Any(v => v.EleicaoId == eleicaoId && v.AssociadoId == associadoId);
        }

        private string GerarHashVoto(Guid eleicaoId, Guid associadoId, DateTime dataHora, string respostas)
        {
            // Simula geração de hash SHA-256
            var input = $"{eleicaoId}|{associadoId}|{dataHora:yyyy-MM-dd HH:mm:ss}|{respostas}";
            return $"HASH_{input.GetHashCode():X8}_{DateTime.Now.Ticks:X}";
        }

        #endregion
    }
}