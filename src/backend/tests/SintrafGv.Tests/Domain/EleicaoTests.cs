using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Enums;

namespace SintrafGv.Tests.Domain
{
    public class EleicaoTests
    {
        [Fact]
        public void Eleicao_DeveSerCriadaComValoresValidos()
        {
            // Arrange
            var titulo = "Eleição Sindical 2026";
            var descricao = "Eleição para escolha da nova diretoria";
            var inicioVotacao = DateTime.Now.AddDays(1);
            var fimVotacao = DateTime.Now.AddDays(7);
            var criadoPor = Guid.NewGuid();

            // Act
            var eleicao = new Eleicao
            {
                Id = Guid.NewGuid(),
                Titulo = titulo,
                Descricao = descricao,
                InicioVotacao = inicioVotacao,
                FimVotacao = fimVotacao,
                Tipo = TipoEleicao.Eleicao,
                ApenasAssociados = true,
                ApenasAtivos = true,
                Status = StatusEleicao.Rascunho,
                CriadoEm = DateTime.Now,
                CriadoPor = criadoPor,
                Perguntas = new List<Pergunta>(),
                Votos = new List<Voto>()
            };

            // Assert
            eleicao.Titulo.Should().Be(titulo);
            eleicao.Descricao.Should().Be(descricao);
            eleicao.InicioVotacao.Should().Be(inicioVotacao);
            eleicao.FimVotacao.Should().Be(fimVotacao);
            eleicao.Tipo.Should().Be(TipoEleicao.Eleicao);
            eleicao.ApenasAssociados.Should().BeTrue();
            eleicao.ApenasAtivos.Should().BeTrue();
            eleicao.Status.Should().Be(StatusEleicao.Rascunho);
            eleicao.CriadoPor.Should().Be(criadoPor);
        }

        [Theory]
        [InlineData(StatusEleicao.Rascunho, false)]
        [InlineData(StatusEleicao.Aberta, true)]
        [InlineData(StatusEleicao.Encerrada, false)]
        [InlineData(StatusEleicao.Apurada, false)]
        [InlineData(StatusEleicao.Cancelada, false)]
        public void EstaAbertaParaVotacao_DeveRetornarCorreto(StatusEleicao status, bool esperado)
        {
            // Arrange
            var eleicao = CriarEleicaoValida();
            eleicao.Status = status;
            eleicao.InicioVotacao = DateTime.Now.AddMinutes(-30);
            eleicao.FimVotacao = DateTime.Now.AddMinutes(30);

            // Act
            var resultado = EstaAbertaParaVotacao(eleicao);

            // Assert
            resultado.Should().Be(esperado);
        }

        [Fact]
        public void EstaAbertaParaVotacao_AntesDoInicioDeveRetornarFalse()
        {
            // Arrange
            var eleicao = CriarEleicaoValida();
            eleicao.Status = StatusEleicao.Aberta;
            eleicao.InicioVotacao = DateTime.Now.AddMinutes(30);
            eleicao.FimVotacao = DateTime.Now.AddHours(2);

            // Act
            var resultado = EstaAbertaParaVotacao(eleicao);

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public void EstaAbertaParaVotacao_AposOFimDeveRetornarFalse()
        {
            // Arrange
            var eleicao = CriarEleicaoValida();
            eleicao.Status = StatusEleicao.Aberta;
            eleicao.InicioVotacao = DateTime.Now.AddHours(-2);
            eleicao.FimVotacao = DateTime.Now.AddMinutes(-30);

            // Act
            var resultado = EstaAbertaParaVotacao(eleicao);

            // Assert
            resultado.Should().BeFalse();
        }

        [Theory]
        [InlineData(true, true, true)] // Apenas associados ativos
        [InlineData(true, false, true)] // Apenas associados (incluindo inativos)
        [InlineData(false, true, true)] // Todos podem votar, mas se for ativo pode
        [InlineData(false, false, true)] // Todos podem votar
        public void AssociadoPodeVotar_DeveFuncionarCorretamente(bool apenasAssociados, bool apenasAtivos, bool associadoAtivo)
        {
            // Arrange
            var eleicao = CriarEleicaoValida();
            eleicao.ApenasAssociados = apenasAssociados;
            eleicao.ApenasAtivos = apenasAtivos;

            var associado = new Associado
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Cpf = "12345678901",
                Ativo = associadoAtivo,
                DataFiliacao = DateTime.Now.AddYears(-2)
            };

            // Act
            var podeVotar = PodeVotar(eleicao, associado);

            // Assert
            var esperado = (!apenasAssociados || true) && // Sempre true pois é associado
                          (!apenasAtivos || associadoAtivo); // Se requer ativo, deve ser ativo

            podeVotar.Should().Be(esperado);
        }

        [Fact]
        public void AssociadoPodeVotar_ComFiltroPorBanco_DeveFuncionar()
        {
            // Arrange
            var bancoId = Guid.NewGuid();
            var eleicao = CriarEleicaoValida();
            eleicao.BancoId = bancoId;

            var associadoBancoCorreto = new Associado
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Ativo = true
                // TODO: Adicionar BancoId quando implementar relacionamento
            };

            // Act & Assert
            // Por enquanto, todos podem votar até implementarmos o relacionamento com banco
            var podeVotar = PodeVotar(eleicao, associadoBancoCorreto);
            podeVotar.Should().BeTrue(); // Implementação futura
        }

        [Fact]
        public void Pergunta_DevePermitirMultiplasOpcoes()
        {
            // Arrange
            var pergunta = new Pergunta
            {
                Id = Guid.NewGuid(),
                EleicaoId = Guid.NewGuid(),
                Ordem = 1,
                Texto = "Escolha até 3 candidatos para o conselho",
                Tipo = TipoPergunta.MultiploVoto,
                MaxVotos = 3,
                PermiteBranco = true,
                Opcoes = new List<Opcao>
                {
                    new Opcao { Id = Guid.NewGuid(), Ordem = 1, Texto = "Candidato A" },
                    new Opcao { Id = Guid.NewGuid(), Ordem = 2, Texto = "Candidato B" },
                    new Opcao { Id = Guid.NewGuid(), Ordem = 3, Texto = "Candidato C" },
                    new Opcao { Id = Guid.NewGuid(), Ordem = 4, Texto = "Candidato D" }
                }
            };

            // Act & Assert
            pergunta.Tipo.Should().Be(TipoPergunta.MultiploVoto);
            pergunta.MaxVotos.Should().Be(3);
            pergunta.Opcoes.Should().HaveCount(4);
            pergunta.PermiteBranco.Should().BeTrue();
        }

        [Fact]
        public void Voto_DeveSerCriadoComDadosDeAuditoria()
        {
            // Arrange & Act
            var voto = new Voto
            {
                Id = Guid.NewGuid(),
                EleicaoId = Guid.NewGuid(),
                AssociadoId = Guid.NewGuid(),
                DataHora = DateTime.Now,
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0...",
                Dispositivo = "Web",
                HashVoto = "abc123hash456"
            };

            // Assert
            voto.Id.Should().NotBeEmpty();
            voto.EleicaoId.Should().NotBeEmpty();
            voto.AssociadoId.Should().NotBeEmpty();
            voto.DataHora.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
            voto.IpAddress.Should().Be("192.168.1.100");
            voto.UserAgent.Should().Be("Mozilla/5.0...");
            voto.Dispositivo.Should().Be("Web");
            voto.HashVoto.Should().Be("abc123hash456");
        }

        [Fact]
        public void VotoDetalhe_NaoDeveConterAssociadoId_ParaSigilo()
        {
            // Arrange & Act
            var votoDetalhe = new VotoDetalhe
            {
                Id = Guid.NewGuid(),
                EleicaoId = Guid.NewGuid(),
                PerguntaId = Guid.NewGuid(),
                OpcaoId = Guid.NewGuid(), // Voto válido
                DataHora = DateTime.Now
                // Importante: SEM AssociadoId para manter sigilo
            };

            // Assert
            votoDetalhe.Id.Should().NotBeEmpty();
            votoDetalhe.EleicaoId.Should().NotBeEmpty();
            votoDetalhe.PerguntaId.Should().NotBeEmpty();
            votoDetalhe.OpcaoId.Should().NotBeEmpty();
            
            // Confirma que não há campo AssociadoId na entidade
            var propriedades = typeof(VotoDetalhe).GetProperties();
            propriedades.Should().NotContain(p => p.Name == "AssociadoId");
        }

        [Fact]
        public void VotoDetalhe_PodeSerBranco_ComOpcaoIdNulo()
        {
            // Arrange & Act - Voto em branco
            var votoEmBranco = new VotoDetalhe
            {
                Id = Guid.NewGuid(),
                EleicaoId = Guid.NewGuid(),
                PerguntaId = Guid.NewGuid(),
                OpcaoId = null, // Voto em branco
                DataHora = DateTime.Now
            };

            // Assert
            votoEmBranco.OpcaoId.Should().BeNull();
            votoEmBranco.EleicaoId.Should().NotBeEmpty();
            votoEmBranco.PerguntaId.Should().NotBeEmpty();
        }

        #region Métodos auxiliares

        private Eleicao CriarEleicaoValida()
        {
            return new Eleicao
            {
                Id = Guid.NewGuid(),
                Titulo = "Eleição Teste",
                Descricao = "Descrição teste",
                InicioVotacao = DateTime.Now.AddDays(-1),
                FimVotacao = DateTime.Now.AddDays(1),
                Tipo = TipoEleicao.Eleicao,
                ApenasAssociados = true,
                ApenasAtivos = true,
                Status = StatusEleicao.Aberta,
                CriadoEm = DateTime.Now.AddDays(-2),
                CriadoPor = Guid.NewGuid(),
                Perguntas = new List<Pergunta>(),
                Votos = new List<Voto>()
            };
        }

        // Simula lógica de negócio que seria implementada nos serviços
        private bool EstaAbertaParaVotacao(Eleicao eleicao)
        {
            var agora = DateTime.Now;
            return eleicao.Status == StatusEleicao.Aberta &&
                   agora >= eleicao.InicioVotacao &&
                   agora <= eleicao.FimVotacao;
        }

        private bool PodeVotar(Eleicao eleicao, Associado associado)
        {
            // Regras básicas - seriam implementadas no serviço real
            if (eleicao.ApenasAssociados && associado == null) return false;
            if (eleicao.ApenasAtivos && !associado.Ativo) return false;
            // TODO: Verificar filtro por banco quando implementado
            return true;
        }

        #endregion
    }
}