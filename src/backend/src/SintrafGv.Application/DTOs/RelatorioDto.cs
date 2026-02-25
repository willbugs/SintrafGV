using System;
using System.Collections.Generic;

namespace SintrafGv.Application.DTOs
{
    public class RelatorioRequest
    {
        public string TipoRelatorio { get; set; } = string.Empty;
        public Dictionary<string, object> Filtros { get; set; } = new();
        public List<string> CamposSelecionados { get; set; } = new();
        public string FormatoExportacao { get; set; } = "html"; // html, pdf, excel, csv
        public string Ordenacao { get; set; } = string.Empty;
        public bool OrdenacaoDecrescente { get; set; } = false;
        public int? Pagina { get; set; }
        public int? TamanhoPagina { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }

    public class RelatorioResponse<T>
    {
        public List<T> Dados { get; set; } = new();
        public RelatorioMetadata Metadata { get; set; } = new();
        public Dictionary<string, object> Totalizadores { get; set; } = new();
    }

    public class RelatorioMetadata
    {
        public string Titulo { get; set; } = string.Empty;
        public string Subtitulo { get; set; } = string.Empty;
        public DateTime DataGeracao { get; set; } = DateTime.Now;
        public int TotalRegistros { get; set; }
        public int? PaginaAtual { get; set; }
        public int? TotalPaginas { get; set; }
        public Dictionary<string, object> FiltrosAplicados { get; set; } = new();
        public List<CampoRelatorio> CamposDisponiveis { get; set; } = new();
    }

    public class CampoRelatorio
    {
        public string Nome { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // string, number, date, boolean
        public bool Totalizavel { get; set; } = false;
        public bool Filtravel { get; set; } = true;
        public bool Ordenavel { get; set; } = true;
        public string? Formato { get; set; }
        public string? Mascara { get; set; }
    }

    public class AssociadoRelatorioDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string? MatriculaSindicato { get; set; }
        public string? MatriculaBancaria { get; set; }
        public string? Sexo { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Endereco { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Naturalidade { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Funcao { get; set; }
        public DateTime? DataAdmissao { get; set; }
        public DateTime? DataFiliacao { get; set; }
        public DateTime? DataDesligamento { get; set; }
        public string? Celular { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? NomeBanco { get; set; }
        public string? Agencia { get; set; }
        public string? Conta { get; set; }
        public bool Ativo { get; set; }
        public bool Associado { get; set; }
        public bool Aposentado { get; set; }
        public string? Motivo { get; set; }
        public int? Idade { get; set; }
        public int? TempoServico { get; set; }
        public int? TempoFiliacao { get; set; }
    }

    public class DashboardKpiDto
    {
        public int TotalAssociados { get; set; }
        public int AssociadosAtivos { get; set; }
        public int AssociadosInativos { get; set; }
        public int NovosMesAtual { get; set; }
        public int DesligadosMesAtual { get; set; }
        public int EnquetesAbertas { get; set; }
        public int EnquetesEncerradas { get; set; }
        public decimal PercentualCrescimento { get; set; }
        public List<DashboardGraficoDto> GraficoPorBanco { get; set; } = new();
        public List<DashboardGraficoDto> GraficoPorIdade { get; set; } = new();
        public List<DashboardGraficoDto> GraficoPorSexo { get; set; } = new();
        public List<DashboardGraficoDto> CrescimentoMensal { get; set; } = new();
    }

    public class DashboardGraficoDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string? Cor { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class FiltroRelatorioDto
    {
        public string Campo { get; set; } = string.Empty;
        public string Operador { get; set; } = "eq"; // eq, ne, gt, lt, gte, lte, contains, startswith, in
        public object? Valor { get; set; }
        public object? ValorAte { get; set; } // Para filtros de range
        public string? Condicao { get; set; } = "and"; // and, or
    }

    public class ExportacaoRelatorioDto
    {
        public string NomeArquivo { get; set; } = string.Empty;
        public string Formato { get; set; } = string.Empty;
        public byte[] Conteudo { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public long TamanhoBytes { get; set; }
        public DateTime DataGeracao { get; set; } = DateTime.Now;
    }
}