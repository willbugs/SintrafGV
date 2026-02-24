using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Enums;

namespace SintrafGv.Tests.Performance
{
    public class VotacaoPerformanceTests
    {
        [Fact]
        public async Task ProcessarMultiplosVotosSimultaneos_DeveSuportarCargaRazoavel()
        {
            // Arrange
            var numeroVotantes = 100;
            var eleicao = CriarEleicaoTeste();
            var associados = CriarAssociadosTeste(numeroVotantes);
            
            var stopwatch = Stopwatch.StartNew();

            // Act - Simular votação simultânea
            var tarefasVotacao = new List<Task<bool>>();

            for (int i = 0; i < numeroVotantes; i++)
            {
                var associado = associados[i];
                var tarefa = Task.Run(() => SimularVoto(eleicao, associado));
                tarefasVotacao.Add(tarefa);
            }

            var resultados = await Task.WhenAll(tarefasVotacao);
            stopwatch.Stop();

            // Assert
            var votosProcessados = resultados.Count(r => r);
            var tempoTotal = stopwatch.ElapsedMilliseconds;
            var votosSegundo = (double)votosProcessados / (tempoTotal / 1000.0);

            votosProcessados.Should().Be(numeroVotantes, "Todos os votos devem ser processados");
            tempoTotal.Should().BeLessThan(5000, "Deve processar 100 votos em menos de 5 segundos");
            votosSegundo.Should().BeGreaterThan(20, "Deve processar pelo menos 20 votos por segundo");

            Console.WriteLine($"Performance: {votosProcessados} votos em {tempoTotal}ms ({votosSegundo:F2} votos/seg)");
        }

        [Fact]
        public async Task ApuracaoResultados_ComGrandeVolumeVotos_DeveSerRapida()
        {
            // Arrange
            var numeroVotos = 1000;
            var eleicao = CriarEleicaoTeste();
            var votosDetalhes = CriarVotosDetalheTeste(eleicao, numeroVotos);

            var stopwatch = Stopwatch.StartNew();

            // Act - Simular apuração
            var resultado = SimularApuracao(eleicao, votosDetalhes);

            stopwatch.Stop();

            // Assert
            var tempoApuracao = stopwatch.ElapsedMilliseconds;
            
            resultado.Should().NotBeNull();
            resultado.TotalVotos.Should().Be(numeroVotos);
            tempoApuracao.Should().BeLessThan(1000, "Apuração de 1000 votos deve levar menos de 1 segundo");

            Console.WriteLine($"Apuração: {numeroVotos} votos processados em {tempoApuracao}ms");
        }

        [Fact]
        public async Task ValidacaoElegibilidade_ComMuitosAssociados_DeveSerRapida()
        {
            // Arrange
            var numeroAssociados = 5000;
            var eleicao = CriarEleicaoTeste();
            var associados = CriarAssociadosTeste(numeroAssociados);

            var stopwatch = Stopwatch.StartNew();

            // Act - Validar elegibilidade de todos
            var resultados = new List<bool>();
            
            foreach (var associado in associados)
            {
                var podeVotar = ValidarElegibilidade(eleicao, associado);
                resultados.Add(podeVotar);
            }

            stopwatch.Stop();

            // Assert
            var tempoValidacao = stopwatch.ElapsedMilliseconds;
            var validacaoSegundo = (double)numeroAssociados / (tempoValidacao / 1000.0);

            resultados.Should().HaveCount(numeroAssociados);
            tempoValidacao.Should().BeLessThan(2000, "Validação de 5000 associados deve levar menos de 2 segundos");
            validacaoSegundo.Should().BeGreaterThan(2500, "Deve validar pelo menos 2500 associados por segundo");

            Console.WriteLine($"Validação: {numeroAssociados} associados em {tempoValidacao}ms ({validacaoSegundo:F0} val/seg)");
        }

        [Fact]
        public async Task GeracaoHash_ComGrandeVolume_DeveSerConsistente()
        {
            // Arrange
            var numeroHashes = 1000;
            var eleicaoId = Guid.NewGuid();
            
            var stopwatch = Stopwatch.StartNew();
            var hashes = new HashSet<string>();

            // Act - Gerar muitos hashes
            for (int i = 0; i < numeroHashes; i++)
            {
                var associadoId = Guid.NewGuid();
                var dataHora = DateTime.Now.AddMilliseconds(i);
                var respostas = $"pergunta1:opcao{i % 3}";
                
                var hash = GerarHashVoto(eleicaoId, associadoId, dataHora, respostas);
                hashes.Add(hash);
            }

            stopwatch.Stop();

            // Assert
            var tempoGeracao = stopwatch.ElapsedMilliseconds;
            var hashesSegundo = (double)numeroHashes / (tempoGeracao / 1000.0);

            hashes.Should().HaveCount(numeroHashes, "Todos os hashes devem ser únicos");
            tempoGeracao.Should().BeLessThan(1000, "Geração de 1000 hashes deve levar menos de 1 segundo");
            hashesSegundo.Should().BeGreaterThan(1000, "Deve gerar pelo menos 1000 hashes por segundo");

            Console.WriteLine($"Hashing: {numeroHashes} hashes únicos em {tempoGeracao}ms ({hashesSegundo:F0} hash/seg)");
        }

        [Theory]
        [InlineData(50, 2000)] // 50 votantes, máximo 2 segundos
        [InlineData(200, 5000)] // 200 votantes, máximo 5 segundos
        [InlineData(500, 10000)] // 500 votantes, máximo 10 segundos
        public async Task CargaEscalavel_DiferentesVolumes_DeveManterPerformance(int numeroVotantes, int tempoMaximoMs)
        {
            // Arrange
            var eleicao = CriarEleicaoTeste();
            var associados = CriarAssociadosTeste(numeroVotantes);

            var stopwatch = Stopwatch.StartNew();

            // Act - Processar votação em lote
            var votosProcessados = 0;
            var tarefas = associados.Select(async associado =>
            {
                var sucesso = SimularVoto(eleicao, associado);
                if (sucesso) Interlocked.Increment(ref votosProcessados);
                return sucesso;
            });

            var resultados = await Task.WhenAll(tarefas);
            stopwatch.Stop();

            // Assert
            var tempoReal = stopwatch.ElapsedMilliseconds;
            var eficiencia = (double)votosProcessados / numeroVotantes * 100;

            votosProcessados.Should().Be(numeroVotantes);
            tempoReal.Should().BeLessThan(tempoMaximoMs, 
                $"Processamento de {numeroVotantes} votos deve levar menos de {tempoMaximoMs}ms");
            eficiencia.Should().Be(100, "Todos os votos devem ser processados com sucesso");

            Console.WriteLine($"Escala {numeroVotantes}: {tempoReal}ms ({eficiencia:F1}% eficiência)");
        }

        [Fact]
        public async Task MemoriaUtilizada_ComGrandeVolume_DeveSerControlada()
        {
            // Arrange
            var numeroVotos = 10000;
            var memoriaInicial = GC.GetTotalMemory(true);

            // Act - Criar muitos objetos de voto
            var votos = new List<VotoDetalhe>();
            for (int i = 0; i < numeroVotos; i++)
            {
                votos.Add(new VotoDetalhe
                {
                    Id = Guid.NewGuid(),
                    EleicaoId = Guid.NewGuid(),
                    PerguntaId = Guid.NewGuid(),
                    OpcaoId = Guid.NewGuid(),
                    DataHora = DateTime.Now
                });
            }

            var memoriaAposVotos = GC.GetTotalMemory(false);
            var usoMemoria = memoriaAposVotos - memoriaInicial;

            // Simular processamento
            var agrupados = votos.GroupBy(v => v.PerguntaId).ToList();
            var contagens = agrupados.SelectMany(g => 
                g.GroupBy(v => v.OpcaoId).Select(og => new { og.Key, Count = og.Count() })).ToList();

            var memoriaFinal = GC.GetTotalMemory(true);
            var memoriaLiberada = memoriaAposVotos - memoriaFinal;

            // Assert
            var bytesVoto = usoMemoria / numeroVotos;
            
            votos.Should().HaveCount(numeroVotos);
            bytesVoto.Should().BeLessThan(1000, "Cada voto não deve usar mais de 1KB de memória");
            usoMemoria.Should().BeLessThan(50 * 1024 * 1024, "Uso total não deve exceder 50MB");
            
            Console.WriteLine($"Memória: {usoMemoria / 1024 / 1024:F2}MB para {numeroVotos} votos ({bytesVoto} bytes/voto)");
            Console.WriteLine($"Liberação: {memoriaLiberada / 1024 / 1024:F2}MB liberados após GC");
        }

        #region Métodos auxiliares

        private Eleicao CriarEleicaoTeste()
        {
            var perguntaId = Guid.NewGuid();
            var opcao1Id = Guid.NewGuid();
            var opcao2Id = Guid.NewGuid();

            return new Eleicao
            {
                Id = Guid.NewGuid(),
                Titulo = "Eleição Performance Test",
                Status = StatusEleicao.Aberta,
                InicioVotacao = DateTime.Now.AddHours(-1),
                FimVotacao = DateTime.Now.AddHours(1),
                ApenasAssociados = true,
                ApenasAtivos = true,
                Perguntas = new List<Pergunta>
                {
                    new Pergunta
                    {
                        Id = perguntaId,
                        Texto = "Candidato preferido",
                        Tipo = TipoPergunta.UnicoVoto,
                        Opcoes = new List<Opcao>
                        {
                            new Opcao { Id = opcao1Id, Texto = "Candidato A" },
                            new Opcao { Id = opcao2Id, Texto = "Candidato B" }
                        }
                    }
                }
            };
        }

        private List<Associado> CriarAssociadosTeste(int quantidade)
        {
            var associados = new List<Associado>();
            for (int i = 0; i < quantidade; i++)
            {
                associados.Add(new Associado
                {
                    Id = Guid.NewGuid(),
                    Nome = $"Associado {i + 1}",
                    Cpf = $"{i:D11}",
                    Ativo = i % 10 != 0 // 90% ativos, 10% inativos
                });
            }
            return associados;
        }

        private List<VotoDetalhe> CriarVotosDetalheTeste(Eleicao eleicao, int quantidade)
        {
            var votos = new List<VotoDetalhe>();
            var random = new Random(42); // Seed fixo para resultados reproduzíveis
            
            var pergunta = eleicao.Perguntas.First();
            var opcoes = pergunta.Opcoes.ToList();

            for (int i = 0; i < quantidade; i++)
            {
                var opcaoEscolhida = random.NextDouble() < 0.95 ? 
                    opcoes[random.Next(opcoes.Count)].Id : (Guid?)null; // 5% votos em branco

                votos.Add(new VotoDetalhe
                {
                    Id = Guid.NewGuid(),
                    EleicaoId = eleicao.Id,
                    PerguntaId = pergunta.Id,
                    OpcaoId = opcaoEscolhida,
                    DataHora = DateTime.Now.AddMilliseconds(-i)
                });
            }

            return votos;
        }

        private bool SimularVoto(Eleicao eleicao, Associado associado)
        {
            // Simular validações e processamento de voto
            if (!ValidarElegibilidade(eleicao, associado)) return false;

            // Simular tempo de processamento
            Thread.Sleep(1); // 1ms por voto

            // Simular geração de hash
            var hash = GerarHashVoto(eleicao.Id, associado.Id, DateTime.Now, "pergunta1:opcao1");
            
            return !string.IsNullOrEmpty(hash);
        }

        private bool ValidarElegibilidade(Eleicao eleicao, Associado associado)
        {
            var agora = DateTime.Now;
            return eleicao.Status == StatusEleicao.Aberta &&
                   agora >= eleicao.InicioVotacao &&
                   agora <= eleicao.FimVotacao &&
                   (!eleicao.ApenasAssociados || associado != null) &&
                   (!eleicao.ApenasAtivos || associado.Ativo);
        }

        private object SimularApuracao(Eleicao eleicao, List<VotoDetalhe> votos)
        {
            // Simular processamento de apuração
            var resultadoPorPergunta = votos
                .GroupBy(v => v.PerguntaId)
                .Select(g => new
                {
                    PerguntaId = g.Key,
                    TotalVotos = g.Count(),
                    VotosPorOpcao = g.GroupBy(v => v.OpcaoId)
                        .Select(og => new { OpcaoId = og.Key, Votos = og.Count() })
                        .ToList()
                })
                .ToList();

            return new
            {
                TotalVotos = votos.Count,
                Perguntas = resultadoPorPergunta
            };
        }

        private string GerarHashVoto(Guid eleicaoId, Guid associadoId, DateTime dataHora, string respostas)
        {
            // Simular geração de hash SHA-256
            var input = $"{eleicaoId}|{associadoId}|{dataHora:yyyy-MM-dd HH:mm:ss.fff}|{respostas}";
            
            // Simular tempo de processamento de hash
            var hash = input.GetHashCode();
            
            return $"SHA256_{Math.Abs(hash):X8}_{DateTime.Now.Ticks % 1000000:X6}";
        }

        #endregion
    }
}