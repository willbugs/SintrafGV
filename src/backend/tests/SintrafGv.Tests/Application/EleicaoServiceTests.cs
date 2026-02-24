using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using SintrafGv.Application.Services;
using SintrafGv.Application.DTOs;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Enums;
using SintrafGv.Domain.Interfaces.Repositories;

namespace SintrafGv.Tests.Application
{
    public class EleicaoServiceTests
    {
        private readonly Mock<IEleicaoRepository> _eleicaoRepositoryMock;
        private readonly Mock<IAssociadoRepository> _associadoRepositoryMock;
        private readonly Mock<IVotoRepository> _votoRepositoryMock;
        private readonly EleicaoService _eleicaoService;

        public EleicaoServiceTests()
        {
            _eleicaoRepositoryMock = new Mock<IEleicaoRepository>();
            _associadoRepositoryMock = new Mock<IAssociadoRepository>();
            _votoRepositoryMock = new Mock<IVotoRepository>();
            
            _eleicaoService = new EleicaoService(
                _eleicaoRepositoryMock.Object,
                _associadoRepositoryMock.Object,
                _votoRepositoryMock.Object
            );
        }

        [Fact]
        public async Task CriarEleicao_ComDadosValidos_DeveRetornarSucesso()
        {
            // Arrange
            var criarEleicaoDto = new CriarEleicaoDto
            {
                Titulo = "Eleição Sindical 2026",
                Descricao = "Eleição para escolha da diretoria",
                InicioVotacao = DateTime.Now.AddDays(1),
                FimVotacao = DateTime.Now.AddDays(7),
                Tipo = TipoEleicao.Eleicao,
                ApenasAssociados = true,
                ApenasAtivos = true,
                Perguntas = new List<CriarPerguntaDto>
                {
                    new CriarPerguntaDto
                    {
                        Texto = "Presidente",
                        Tipo = TipoPergunta.UnicoVoto,
                        PermiteBranco = true,
                        Opcoes = new List<CriarOpcaoDto>
                        {
                            new CriarOpcaoDto { Texto = "Candidato A", Descricao = "Experiência em gestão" },
                            new CriarOpcaoDto { Texto = "Candidato B", Descricao = "Foco na modernização" }
                        }
                    }
                }
            };

            var criadoPor = Guid.NewGuid();
            var eleicaoId = Guid.NewGuid();

            _eleicaoRepositoryMock
                .Setup(x => x.AdicionarAsync(It.IsAny<Eleicao>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback<Eleicao, CancellationToken>((e, ct) => e.Id = eleicaoId);

            // Act
            var resultado = await _eleicaoService.CriarEleicaoAsync(criarEleicaoDto, criadoPor);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(eleicaoId);
            resultado.Titulo.Should().Be(criarEleicaoDto.Titulo);
            resultado.Status.Should().Be(StatusEleicao.Rascunho);
            resultado.Perguntas.Should().HaveCount(1);
            resultado.Perguntas.First().Opcoes.Should().HaveCount(2);

            _eleicaoRepositoryMock.Verify(
                x => x.AdicionarAsync(It.IsAny<Eleicao>(), It.IsAny<CancellationToken>()), 
                Times.Once
            );
        }

        [Fact]
        public async Task ValidarElegibilidadeVoto_AssociadoInativo_QuandoRequerAtivos_DeveRetornarErro()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();

            var eleicao = new Eleicao
            {
                Id = eleicaoId,
                Status = StatusEleicao.Aberta,
                InicioVotacao = DateTime.Now.AddHours(-1),
                FimVotacao = DateTime.Now.AddHours(1),
                ApenasAssociados = true,
                ApenasAtivos = true
            };

            var associado = new Associado
            {
                Id = associadoId,
                Nome = "João Silva",
                Ativo = false // Inativo
            };

            _eleicaoRepositoryMock
                .Setup(x => x.ObterPorIdAsync(eleicaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eleicao);

            _associadoRepositoryMock
                .Setup(x => x.ObterPorIdAsync(associadoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(associado);

            // Act
            var resultado = await _eleicaoService.ValidarElegibilidadeVotoAsync(eleicaoId, associadoId);

            // Assert
            resultado.Sucesso.Should().BeFalse();
            resultado.Erros.Should().Contain("Apenas associados ativos podem votar nesta eleição");
        }

        [Fact]
        public async Task ValidarElegibilidadeVoto_ForaDoPeriodo_DeveRetornarErro()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();

            var eleicao = new Eleicao
            {
                Id = eleicaoId,
                Status = StatusEleicao.Aberta,
                InicioVotacao = DateTime.Now.AddDays(1), // Inicia amanhã
                FimVotacao = DateTime.Now.AddDays(7),
                ApenasAssociados = true,
                ApenasAtivos = true
            };

            var associado = new Associado
            {
                Id = associadoId,
                Nome = "João Silva",
                Ativo = true
            };

            _eleicaoRepositoryMock
                .Setup(x => x.ObterPorIdAsync(eleicaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eleicao);

            _associadoRepositoryMock
                .Setup(x => x.ObterPorIdAsync(associadoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(associado);

            // Act
            var resultado = await _eleicaoService.ValidarElegibilidadeVotoAsync(eleicaoId, associadoId);

            // Assert
            resultado.Sucesso.Should().BeFalse();
            resultado.Erros.Should().Contain("Votação ainda não iniciou ou já foi encerrada");
        }

        [Fact]
        public async Task ValidarElegibilidadeVoto_JaVotou_DeveRetornarErro()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();

            var eleicao = new Eleicao
            {
                Id = eleicaoId,
                Status = StatusEleicao.Aberta,
                InicioVotacao = DateTime.Now.AddHours(-1),
                FimVotacao = DateTime.Now.AddHours(1),
                ApenasAssociados = true,
                ApenasAtivos = true
            };

            var associado = new Associado
            {
                Id = associadoId,
                Nome = "João Silva",
                Ativo = true
            };

            _eleicaoRepositoryMock
                .Setup(x => x.ObterPorIdAsync(eleicaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eleicao);

            _associadoRepositoryMock
                .Setup(x => x.ObterPorIdAsync(associadoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(associado);

            // Simula que já votou
            _votoRepositoryMock
                .Setup(x => x.JaVotouAsync(eleicaoId, associadoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var resultado = await _eleicaoService.ValidarElegibilidadeVotoAsync(eleicaoId, associadoId);

            // Assert
            resultado.Sucesso.Should().BeFalse();
            resultado.Erros.Should().Contain("Associado já votou nesta eleição");
        }

        [Fact]
        public async Task ValidarElegibilidadeVoto_CondicoesValidas_DeveRetornarSucesso()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();

            var eleicao = new Eleicao
            {
                Id = eleicaoId,
                Status = StatusEleicao.Aberta,
                InicioVotacao = DateTime.Now.AddHours(-1),
                FimVotacao = DateTime.Now.AddHours(1),
                ApenasAssociados = true,
                ApenasAtivos = true
            };

            var associado = new Associado
            {
                Id = associadoId,
                Nome = "João Silva",
                Ativo = true
            };

            _eleicaoRepositoryMock
                .Setup(x => x.ObterPorIdAsync(eleicaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eleicao);

            _associadoRepositoryMock
                .Setup(x => x.ObterPorIdAsync(associadoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(associado);

            _votoRepositoryMock
                .Setup(x => x.JaVotouAsync(eleicaoId, associadoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var resultado = await _eleicaoService.ValidarElegibilidadeVotoAsync(eleicaoId, associadoId);

            // Assert
            resultado.Sucesso.Should().BeTrue();
            resultado.Erros.Should().BeEmpty();
        }

        [Fact]
        public async Task ProcessarVoto_ComVotoValidoUnicaEscolha_DeveRetornarSucesso()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var associadoId = Guid.NewGuid();
            var perguntaId = Guid.NewGuid();
            var opcaoId = Guid.NewGuid();

            var votoDto = new ProcessarVotoDto
            {
                EleicaoId = eleicaoId,
                AssociadoId = associadoId,
                Respostas = new List<RespostaVotoDto>
                {
                    new RespostaVotoDto
                    {
                        PerguntaId = perguntaId,
                        OpcoesSelecionadas = new List<Guid> { opcaoId }
                    }
                },
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0...",
                Dispositivo = "Web"
            };

            var pergunta = new Pergunta
            {
                Id = perguntaId,
                Tipo = TipoPergunta.UnicoVoto,
                MaxVotos = null,
                Opcoes = new List<Opcao>
                {
                    new Opcao { Id = opcaoId, Texto = "Candidato A" }
                }
            };

            _eleicaoRepositoryMock
                .Setup(x => x.ObterComPerguntasAsync(eleicaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Eleicao
                {
                    Id = eleicaoId,
                    Perguntas = new List<Pergunta> { pergunta }
                });

            _votoRepositoryMock
                .Setup(x => x.AdicionarAsync(It.IsAny<Voto>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _votoRepositoryMock
                .Setup(x => x.AdicionarDetalheAsync(It.IsAny<VotoDetalhe>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _eleicaoService.ProcessarVotoAsync(votoDto);

            // Assert
            resultado.Sucesso.Should().BeTrue();
            resultado.HashVoto.Should().NotBeNullOrEmpty();
            resultado.DataHoraVoto.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));

            _votoRepositoryMock.Verify(
                x => x.AdicionarAsync(It.IsAny<Voto>(), It.IsAny<CancellationToken>()), 
                Times.Once
            );

            _votoRepositoryMock.Verify(
                x => x.AdicionarDetalheAsync(It.IsAny<VotoDetalhe>(), It.IsAny<CancellationToken>()), 
                Times.Once
            );
        }

        [Fact]
        public async Task ProcessarVoto_MultiplaEscolhaExcedendoLimite_DeveRetornarErro()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var perguntaId = Guid.NewGuid();
            var opcao1Id = Guid.NewGuid();
            var opcao2Id = Guid.NewGuid();
            var opcao3Id = Guid.NewGuid();
            var opcao4Id = Guid.NewGuid();

            var votoDto = new ProcessarVotoDto
            {
                EleicaoId = eleicaoId,
                AssociadoId = Guid.NewGuid(),
                Respostas = new List<RespostaVotoDto>
                {
                    new RespostaVotoDto
                    {
                        PerguntaId = perguntaId,
                        OpcoesSelecionadas = new List<Guid> { opcao1Id, opcao2Id, opcao3Id, opcao4Id } // 4 opções
                    }
                }
            };

            var pergunta = new Pergunta
            {
                Id = perguntaId,
                Tipo = TipoPergunta.MultiploVoto,
                MaxVotos = 3, // Limite máximo: 3
                Opcoes = new List<Opcao>
                {
                    new Opcao { Id = opcao1Id },
                    new Opcao { Id = opcao2Id },
                    new Opcao { Id = opcao3Id },
                    new Opcao { Id = opcao4Id }
                }
            };

            _eleicaoRepositoryMock
                .Setup(x => x.ObterComPerguntasAsync(eleicaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Eleicao
                {
                    Id = eleicaoId,
                    Perguntas = new List<Pergunta> { pergunta }
                });

            // Act
            var resultado = await _eleicaoService.ProcessarVotoAsync(votoDto);

            // Assert
            resultado.Sucesso.Should().BeFalse();
            resultado.Erros.Should().Contain("Número de opções selecionadas excede o limite permitido");
        }

        [Fact]
        public async Task ObterResultados_DeveCalcularVotosCorretamente()
        {
            // Arrange
            var eleicaoId = Guid.NewGuid();
            var perguntaId = Guid.NewGuid();
            var opcao1Id = Guid.NewGuid();
            var opcao2Id = Guid.NewGuid();

            var resultados = new List<VotoDetalhe>
            {
                new VotoDetalhe { PerguntaId = perguntaId, OpcaoId = opcao1Id },
                new VotoDetalhe { PerguntaId = perguntaId, OpcaoId = opcao1Id },
                new VotoDetalhe { PerguntaId = perguntaId, OpcaoId = opcao2Id },
                new VotoDetalhe { PerguntaId = perguntaId, OpcaoId = null } // Voto em branco
            };

            _votoRepositoryMock
                .Setup(x => x.ObterResultadosAsync(eleicaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultados);

            _eleicaoRepositoryMock
                .Setup(x => x.ObterComPerguntasAsync(eleicaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Eleicao
                {
                    Id = eleicaoId,
                    Titulo = "Eleição Teste",
                    Perguntas = new List<Pergunta>
                    {
                        new Pergunta
                        {
                            Id = perguntaId,
                            Texto = "Presidente",
                            Opcoes = new List<Opcao>
                            {
                                new Opcao { Id = opcao1Id, Texto = "Candidato A" },
                                new Opcao { Id = opcao2Id, Texto = "Candidato B" }
                            }
                        }
                    }
                });

            // Act
            var resultado = await _eleicaoService.ObterResultadosAsync(eleicaoId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.TotalVotos.Should().Be(4);
            resultado.Perguntas.Should().HaveCount(1);

            var perguntaResultado = resultado.Perguntas.First();
            perguntaResultado.Opcoes.Should().HaveCount(2);
            
            var opcao1Resultado = perguntaResultado.Opcoes.First(o => o.OpcaoId == opcao1Id);
            opcao1Resultado.TotalVotos.Should().Be(2);
            opcao1Resultado.PercentualVotos.Should().Be(50); // 2 de 4 votos válidos

            var opcao2Resultado = perguntaResultado.Opcoes.First(o => o.OpcaoId == opcao2Id);
            opcao2Resultado.TotalVotos.Should().Be(1);
            opcao2Resultado.PercentualVotos.Should().Be(25); // 1 de 4 votos válidos

            perguntaResultado.VotosBranco.Should().Be(1);
        }
    }
}