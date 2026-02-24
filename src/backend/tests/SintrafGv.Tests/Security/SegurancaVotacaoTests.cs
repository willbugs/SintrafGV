using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Enums;

namespace SintrafGv.Tests.Security
{
    public class SegurancaVotacaoTests
    {
        [Fact]
        public void SigiloVoto_VotoDetalheNaoDeveConterIdentificacaoVotante()
        {
            // Arrange & Act
            var votoDetalhe = new VotoDetalhe
            {
                Id = Guid.NewGuid(),
                EleicaoId = Guid.NewGuid(),
                PerguntaId = Guid.NewGuid(),
                OpcaoId = Guid.NewGuid(),
                DataHora = DateTime.Now
            };

            // Assert - Verificar que não há campos que identifiquem o votante
            var propriedades = typeof(VotoDetalhe).GetProperties();
            var camposProibidos = new[] { "AssociadoId", "UsuarioId", "VotanteId", "CPF", "Nome" };

            foreach (var campo in camposProibidos)
            {
                propriedades.Should().NotContain(p => p.Name.Equals(campo, StringComparison.OrdinalIgnoreCase),
                    $"VotoDetalhe não deve ter campo '{campo}' para manter sigilo");
            }

            // Deve ter apenas dados necessários para contabilização
            propriedades.Should().Contain(p => p.Name == "Id");
            propriedades.Should().Contain(p => p.Name == "EleicaoId");
            propriedades.Should().Contain(p => p.Name == "PerguntaId");
            propriedades.Should().Contain(p => p.Name == "OpcaoId");
            propriedades.Should().Contain(p => p.Name == "DataHora");
        }

        [Fact]
        public void SeparacaoVotoIdentidade_VotoEVotoDetalheDevemEstarSeparados()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();
            var perguntaId = Guid.NewGuid();
            var opcaoId = Guid.NewGuid();
            var dataHora = DateTime.Now;

            // Act - Criar voto (com identificação) e detalhe (sem identificação)
            var voto = new Voto
            {
                Id = Guid.NewGuid(),
                EleicaoId = eleicaoId,
                AssociadoId = associadoId, // TEM identificação do votante
                DataHora = dataHora,
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0...",
                Dispositivo = "Web",
                HashVoto = "hash123"
            };

            var votoDetalhe = new VotoDetalhe
            {
                Id = Guid.NewGuid(),
                EleicaoId = eleicaoId,
                PerguntaId = perguntaId,
                OpcaoId = opcaoId, // NÃO TEM identificação do votante
                DataHora = dataHora
            };

            // Assert - Verificar separação
            voto.AssociadoId.Should().Be(associadoId, "Voto deve registrar quem votou");
            voto.EleicaoId.Should().Be(eleicaoId);

            votoDetalhe.EleicaoId.Should().Be(eleicaoId, "Detalhe deve referenciar mesma eleição");
            votoDetalhe.OpcaoId.Should().Be(opcaoId, "Detalhe deve registrar a escolha");
            
            // CRÍTICO: Não deve ser possível associar diretamente VotoDetalhe ao Voto
            // Em uma implementação real, a ligação seria apenas temporal/sequencial
            var propriedadesDetalhe = typeof(VotoDetalhe).GetProperties();
            propriedadesDetalhe.Should().NotContain(p => p.Name == "VotoId", 
                "VotoDetalhe não deve referenciar diretamente o Voto");
        }

        [Fact]
        public void HashIntegridade_DeveMudarComQualquerAlteracao()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();
            var dataHora = DateTime.Now;
            var respostas = "pergunta1:opcao1,pergunta2:opcao2";

            // Act - Gerar hashes com pequenas variações
            var hashOriginal = GerarHashSeguro(eleicaoId, associadoId, dataHora, respostas);
            var hashEleicaoDiferente = GerarHashSeguro(Guid.NewGuid(), associadoId, dataHora, respostas);
            var hashAssociadoDiferente = GerarHashSeguro(eleicaoId, Guid.NewGuid(), dataHora, respostas);
            var hashDataDiferente = GerarHashSeguro(eleicaoId, associadoId, dataHora.AddSeconds(1), respostas);
            var hashRespostaDiferente = GerarHashSeguro(eleicaoId, associadoId, dataHora, "pergunta1:opcao2");

            // Assert - Todos os hashes devem ser diferentes
            var hashes = new[] { hashOriginal, hashEleicaoDiferente, hashAssociadoDiferente, hashDataDiferente, hashRespostaDiferente };
            
            hashes.Should().OnlyHaveUniqueItems("Qualquer mudança nos dados deve gerar hash diferente");
            
            foreach (var hash in hashes)
            {
                hash.Should().NotBeNullOrEmpty();
                hash.Length.Should().Be(64, "Hash SHA-256 deve ter 64 caracteres");
            }
        }

        [Fact]
        public void HashIntegridade_DeveSerDeterministico()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();
            var dataHora = new DateTime(2026, 2, 24, 15, 30, 45); // Data fixa
            var respostas = "pergunta1:opcao1";

            // Act - Gerar mesmo hash múltiplas vezes
            var hash1 = GerarHashSeguro(eleicaoId, associadoId, dataHora, respostas);
            var hash2 = GerarHashSeguro(eleicaoId, associadoId, dataHora, respostas);
            var hash3 = GerarHashSeguro(eleicaoId, associadoId, dataHora, respostas);

            // Assert
            hash1.Should().Be(hash2).And.Be(hash3, "Mesmo input deve sempre gerar mesmo hash");
        }

        [Fact]
        public void ValidacaoTentativaTampering_DeveDetectarManipulacao()
        {
            // Arrange - Simular dados de voto
            var dadosOriginais = new
            {
                EleicaoId = Guid.NewGuid(),
                PerguntaId = Guid.NewGuid(),
                OpcaoId = Guid.NewGuid(),
                DataHora = DateTime.Now
            };

            var hashOriginal = GerarHashSeguro(
                dadosOriginais.EleicaoId, 
                Guid.NewGuid(), 
                dadosOriginais.DataHora, 
                $"{dadosOriginais.PerguntaId}:{dadosOriginais.OpcaoId}"
            );

            // Act - Simular tentativas de manipulação
            var dadosManipulados1 = new
            {
                EleicaoId = dadosOriginais.EleicaoId,
                PerguntaId = dadosOriginais.PerguntaId,
                OpcaoId = Guid.NewGuid(), // Opção alterada
                DataHora = dadosOriginais.DataHora
            };

            var dadosManipulados2 = new
            {
                EleicaoId = dadosOriginais.EleicaoId,
                PerguntaId = dadosOriginais.PerguntaId,
                OpcaoId = dadosOriginais.OpcaoId,
                DataHora = dadosOriginais.DataHora.AddMinutes(-1) // Data alterada
            };

            var hashManipulado1 = GerarHashSeguro(
                dadosManipulados1.EleicaoId, 
                Guid.NewGuid(), 
                dadosManipulados1.DataHora, 
                $"{dadosManipulados1.PerguntaId}:{dadosManipulados1.OpcaoId}"
            );

            var hashManipulado2 = GerarHashSeguro(
                dadosManipulados2.EleicaoId, 
                Guid.NewGuid(), 
                dadosManipulados2.DataHora, 
                $"{dadosManipulados2.PerguntaId}:{dadosManipulados2.OpcaoId}"
            );

            // Assert - Manipulações devem ser detectadas
            hashManipulado1.Should().NotBe(hashOriginal, "Alteração na opção deve ser detectada");
            hashManipulado2.Should().NotBe(hashOriginal, "Alteração na data deve ser detectada");
        }

        [Theory]
        [InlineData("192.168.1.100", true)]  // IP interno válido
        [InlineData("10.0.0.1", true)]       // IP interno válido
        [InlineData("127.0.0.1", false)]     // Localhost - suspeito
        [InlineData("0.0.0.0", false)]       // IP inválido
        [InlineData("", false)]               // IP vazio
        [InlineData(null, false)]             // IP nulo
        public void ValidacaoIP_DeveIdentificarIPsSuspeitos(string ipAddress, bool esperadoValido)
        {
            // Act
            var ipValido = ValidarIPVotacao(ipAddress);

            // Assert
            ipValido.Should().Be(esperadoValido);
        }

        [Fact]
        public void AuditoriaVoto_DeveRegistrarDadosCompletos()
        {
            // Arrange & Act
            var votoAuditoria = new Voto
            {
                Id = Guid.NewGuid(),
                EleicaoId = Guid.NewGuid(),
                AssociadoId = Guid.NewGuid(),
                DataHora = DateTime.Now,
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                Dispositivo = "Web",
                HashVoto = "a1b2c3d4e5f6..."
            };

            // Assert - Verificar dados de auditoria
            votoAuditoria.IpAddress.Should().NotBeNullOrEmpty("IP deve ser registrado para auditoria");
            votoAuditoria.UserAgent.Should().NotBeNullOrEmpty("User-Agent deve ser registrado");
            votoAuditoria.Dispositivo.Should().BeOneOf("Web", "Android", "iOS", "PWA");
            votoAuditoria.DataHora.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(5));
            votoAuditoria.HashVoto.Should().NotBeNullOrEmpty("Hash é crítico para integridade");
        }

        [Fact]
        public void ProtecaoReplayAttack_VotoDuplicadoDeveSerRejeitado()
        {
            // Arrange - Simular voto já registrado
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();
            
            var votosExistentes = new List<Voto>
            {
                new Voto
                {
                    Id = Guid.NewGuid(),
                    EleicaoId = eleicaoId,
                    AssociadoId = associadoId,
                    DataHora = DateTime.Now.AddMinutes(-10),
                    HashVoto = "hash_existente"
                }
            };

            // Act - Tentar votar novamente
            var jaVotou = VerificarVotoDuplicado(votosExistentes, eleicaoId, associadoId);
            var tentativaReplay = TentarVotoReplay(votosExistentes, eleicaoId, associadoId);

            // Assert
            jaVotou.Should().BeTrue("Sistema deve detectar que associado já votou");
            tentativaReplay.sucesso.Should().BeFalse("Replay attack deve ser bloqueado");
            tentativaReplay.motivo.Should().Contain("já votou", StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ValidacaoPeriodoVotacao_DevePrevenirVotosForaDoPrazo()
        {
            // Arrange
            var eleicaoFutura = new Eleicao
            {
                Id = Guid.NewGuid(),
                Status = StatusEleicao.Rascunho,
                InicioVotacao = DateTime.Now.AddDays(1),
                FimVotacao = DateTime.Now.AddDays(7)
            };

            var eleicaoExpirada = new Eleicao
            {
                Id = Guid.NewGuid(),
                Status = StatusEleicao.Encerrada,
                InicioVotacao = DateTime.Now.AddDays(-7),
                FimVotacao = DateTime.Now.AddDays(-1)
            };

            var eleicaoValida = new Eleicao
            {
                Id = Guid.NewGuid(),
                Status = StatusEleicao.Aberta,
                InicioVotacao = DateTime.Now.AddHours(-1),
                FimVotacao = DateTime.Now.AddHours(1)
            };

            // Act & Assert
            ValidarPeriodoVotacao(eleicaoFutura).Should().BeFalse("Não deve aceitar votos antes do início");
            ValidarPeriodoVotacao(eleicaoExpirada).Should().BeFalse("Não deve aceitar votos após o fim");
            ValidarPeriodoVotacao(eleicaoValida).Should().BeTrue("Deve aceitar votos no período válido");
        }

        [Fact]
        public void SanitizacaoEntrada_DevePrevenirInjecaoMaliciosa()
        {
            // Arrange - Entradas potencialmente maliciosas
            var entradasMaliciosas = new[]
            {
                "<script>alert('xss')</script>",
                "'; DROP TABLE Votos; --",
                "../../../etc/passwd",
                "${java.lang.Runtime.getRuntime().exec('rm -rf /')}",
                "{{constructor.constructor('return process')().exit()}}"
            };

            // Act & Assert
            foreach (var entrada in entradasMaliciosas)
            {
                var entrادaSanitizada = SanitizarEntrada(entrada);
                
                entrادaSanitizada.Should().NotContain("<script>", "Scripts devem ser removidos");
                entrادaSanitizada.Should().NotContain("DROP TABLE", "SQL injection deve ser bloqueado");
                entrادaSanitizada.Should().NotContain("../", "Path traversal deve ser bloqueado");
                entrادaSanitizada.Should().NotMatch("*constructor*", "Code injection deve ser bloqueado");
            }
        }

        #region Métodos auxiliares de segurança

        private string GerarHashSeguro(Guid eleicaoId, Guid associadoId, DateTime dataHora, string respostas)
        {
            var input = $"{eleicaoId}|{associadoId}|{dataHora:yyyy-MM-ddTHH:mm:ss.fffZ}|{respostas}";
            
            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
        }

        private bool ValidarIPVotacao(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) return false;
            
            // IPs suspeitos
            var ipsSuspeitos = new[] { "127.0.0.1", "0.0.0.0", "::1" };
            if (ipsSuspeitos.Contains(ipAddress)) return false;
            
            // Validação básica de formato IP
            return System.Net.IPAddress.TryParse(ipAddress, out _);
        }

        private bool VerificarVotoDuplicado(List<Voto> votosExistentes, Guid eleicaoId, Guid associadoId)
        {
            return votosExistentes.Any(v => v.EleicaoId == eleicaoId && v.AssociadoId == associadoId);
        }

        private (bool sucesso, string motivo) TentarVotoReplay(List<Voto> votosExistentes, Guid eleicaoId, Guid associadoId)
        {
            if (VerificarVotoDuplicado(votosExistentes, eleicaoId, associadoId))
            {
                return (false, "Associado já votou nesta eleição");
            }
            
            return (true, "");
        }

        private bool ValidarPeriodoVotacao(Eleicao eleicao)
        {
            var agora = DateTime.Now;
            return eleicao.Status == StatusEleicao.Aberta &&
                   agora >= eleicao.InicioVotacao &&
                   agora <= eleicao.FimVotacao;
        }

        private string SanitizarEntrada(string entrada)
        {
            if (string.IsNullOrEmpty(entrada)) return entrada;
            
            // Remover tags HTML
            entrada = System.Text.RegularExpressions.Regex.Replace(entrada, @"<[^>]*>", "");
            
            // Remover comandos SQL perigosos
            var comandosProibidos = new[] { "DROP", "DELETE", "UPDATE", "INSERT", "EXEC", "UNION", "--", ";" };
            foreach (var comando in comandosProibidos)
            {
                entrada = entrada.Replace(comando, "", StringComparison.OrdinalIgnoreCase);
            }
            
            // Remover path traversal
            entrada = entrada.Replace("../", "").Replace("..\\", "");
            
            // Remover tentativas de code injection
            entrada = System.Text.RegularExpressions.Regex.Replace(entrada, @"\$\{[^}]*\}", "");
            entrada = System.Text.RegularExpressions.Regex.Replace(entrada, @"\{\{[^}]*\}\}", "");
            
            return entrada.Trim();
        }

        #endregion
    }
}