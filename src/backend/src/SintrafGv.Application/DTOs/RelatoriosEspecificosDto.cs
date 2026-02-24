using System;
using System.Collections.Generic;

namespace SintrafGv.Application.DTOs
{
    // DTO para Relatório de Inadimplência
    public class InadimplenciaDto
    {
        public Guid AssociadoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string MatriculaSindicato { get; set; } = string.Empty;
        public string MatriculaBancaria { get; set; } = string.Empty;
        public string NomeBanco { get; set; } = string.Empty;
        public int MesesAtraso { get; set; }
        public DateTime? UltimoPagamento { get; set; }
        public decimal ValorDevido { get; set; }
        public string Telefone { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataFiliacao { get; set; }
        public string StatusAssociado { get; set; } = string.Empty;
    }

    // DTO para Relatório de Movimentação Mensal
    public class MovimentacaoMensalDto
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string MesNome { get; set; } = string.Empty;
        public int NovasFiliacao { get; set; }
        public int Desligamentos { get; set; }
        public int SaldoMovimentacao { get; set; }
        public int TotalAtivosFinalizados { get; set; }
        public decimal PercentualCrescimento { get; set; }
        
        // Detalhes dos novos filiados
        public List<NovoFiliadoDto> NovosFiliados { get; set; } = new();
        
        // Detalhes dos desligamentos
        public List<DesligamentoDto> DetalhesDesligamentos { get; set; } = new();
    }

    public class NovoFiliadoDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public DateTime DataFiliacao { get; set; }
        public string NomeBanco { get; set; } = string.Empty;
        public string Funcao { get; set; } = string.Empty;
    }

    public class DesligamentoDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public DateTime DataDesligamento { get; set; }
        public string MotivoDesligamento { get; set; } = string.Empty;
        public string NomeBanco { get; set; } = string.Empty;
        public TimeSpan TempoFiliacao { get; set; }
    }

    // DTO para Relatório de Participação em Votações
    public class ParticipacaoVotacaoDto
    {
        public Guid AssociadoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string MatriculaSindicato { get; set; } = string.Empty;
        public string NomeBanco { get; set; } = string.Empty;
        public string Funcao { get; set; } = string.Empty;
        public int TotalEleicoesDisponiveis { get; set; }
        public int TotalVotosRealizados { get; set; }
        public decimal PercentualParticipacao { get; set; }
        public DateTime? UltimaVotacao { get; set; }
        public string UltimaEleicaoTitulo { get; set; } = string.Empty;
        public string StatusAssociado { get; set; } = string.Empty;
        public DateTime DataFiliacao { get; set; }
    }

    // DTO para Relatório de Distribuição por Faixa Etária
    public class FaixaEtariaDto
    {
        public string FaixaEtaria { get; set; } = string.Empty; // "18-30", "31-45", etc.
        public int IdadeMinima { get; set; }
        public int IdadeMaxima { get; set; }
        public int TotalAssociados { get; set; }
        public int AssociadosAtivos { get; set; }
        public int AssociadosInativos { get; set; }
        public decimal PercentualTotal { get; set; }
        public decimal IdadeMedia { get; set; }
        public List<AssociadoFaixaEtariaDto> Detalhes { get; set; } = new();
    }

    public class AssociadoFaixaEtariaDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public int Idade { get; set; }
        public string NomeBanco { get; set; } = string.Empty;
        public string Funcao { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataFiliacao { get; set; }
    }

    // DTO para Relatório de Aposentados e Pensionistas
    public class AposentadoPensionistaDto
    {
        public Guid AssociadoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string MatriculaSindicato { get; set; } = string.Empty;
        public string MatriculaBancaria { get; set; } = string.Empty;
        public string NomeBanco { get; set; } = string.Empty;
        public TipoBeneficio TipoBeneficio { get; set; }
        public string TipoBeneficioDescricao => TipoBeneficio.ToString();
        public DateTime? DataAposentadoria { get; set; }
        public DateTime? DataPensao { get; set; }
        public bool Ativo { get; set; }
        public string StatusAssociado { get; set; } = string.Empty;
        public DateTime DataFiliacao { get; set; }
        public DateTime? DataDesligamento { get; set; }
        public int IdadeAtual { get; set; }
        public TimeSpan TempoContribuicao { get; set; }
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public enum TipoBeneficio
    {
        Aposentado,
        Pensionista,
        AposentadoPensionista,
        Ativo
    }

    // DTO para Resumo de Relatórios
    public class ResumoRelatorioDto
    {
        public string TipoRelatorio { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public DateTime DataGeracao { get; set; }
        public int TotalRegistros { get; set; }
        public Dictionary<string, object> Totalizadores { get; set; } = new();
        public List<GraficoDto> Graficos { get; set; } = new();
    }

    public class GraficoDto
    {
        public string Tipo { get; set; } = string.Empty; // "bar", "pie", "line"
        public string Titulo { get; set; } = string.Empty;
        public List<string> Labels { get; set; } = new();
        public List<decimal> Valores { get; set; } = new();
        public List<string> Cores { get; set; } = new();
    }
}