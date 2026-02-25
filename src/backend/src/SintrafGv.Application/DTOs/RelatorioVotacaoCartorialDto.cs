using System;
using System.Collections.Generic;

namespace SintrafGv.Application.DTOs
{
    // Relatório Completo de Votação para Cartório
    public class RelatorioVotacaoCartorialDto
    {
        // Dados do Sindicato
        public DadosSindicatoDto DadosSindicato { get; set; } = new();
        
        // Dados da Eleição/Enquete
        public DadosEleicaoDto DadosEleicao { get; set; } = new();
        
        // Lista de Votos com Timestamp
        public List<VotoDetalhado> Votos { get; set; } = new();
        
        // Resumo Estatístico
        public ResumoVotacaoDto Resumo { get; set; } = new();
        
        // Dados para Autenticação
        public DadosAutenticacaoDto Autenticacao { get; set; } = new();
    }

    public class DadosSindicatoDto
    {
        public string RazaoSocial { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string InscricaoEstadual { get; set; } = string.Empty;
        public string EnderecoCompleto { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Presidente { get; set; } = string.Empty;
        public string CPFPresidente { get; set; } = string.Empty;
        public string? Secretario { get; set; }
        public string? CPFSecretario { get; set; }
    }

    public class DadosEleicaoDto
    {
        public Guid EleicaoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public string HoraInicio { get; set; } = string.Empty;
        public DateTime DataFim { get; set; }
        public string HoraFim { get; set; } = string.Empty;
        public string Pergunta { get; set; } = string.Empty;
        public List<string> Opcoes { get; set; } = new();
        public bool ApenasAssociados { get; set; }
        public string? ArquivoAnexo { get; set; }
    }

    public class VotoDetalhado
    {
        public Guid VotoId { get; set; }
        public DateTime DataHoraVoto { get; set; }
        public string TimestampPreciso { get; set; } = string.Empty; // Com milissegundos
        
        // Dados do Votante (Anonimizados para relatório público)
        public string CodigoVotante { get; set; } = string.Empty; // Hash ou código
        public string NomeVotante { get; set; } = string.Empty;
        public string CPFVotante { get; set; } = string.Empty;
        public string MatriculaSindicato { get; set; } = string.Empty;
        public string MatriculaBancaria { get; set; } = string.Empty;
        public string Banco { get; set; } = string.Empty;
        
        // Dados do Voto
        public string Pergunta { get; set; } = string.Empty;
        public string RespostaSelecionada { get; set; } = string.Empty;
        public int NumeroResposta { get; set; }
        
        // Dados Técnicos para Auditoria
        public string EnderecoIP { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string HashVoto { get; set; } = string.Empty; // Hash criptográfico do voto
        public string AssinaturaDigital { get; set; } = string.Empty; // Assinatura digital
    }

    public class ResumoVotacaoDto
    {
        public int TotalVotosComputados { get; set; }
        public int TotalAssociadosAptos { get; set; }
        public decimal PercentualParticipacao { get; set; }
        public Dictionary<string, int> ResultadoPorOpcao { get; set; } = new();
        public Dictionary<string, decimal> PercentualPorOpcao { get; set; } = new();
        public DateTime PrimeiroVoto { get; set; }
        public DateTime UltimoVoto { get; set; }
        public TimeSpan DuracaoVotacao { get; set; }
    }

    public class DadosAutenticacaoDto
    {
        public DateTime DataGeracaoRelatorio { get; set; }
        public string HoraGeracaoRelatorio { get; set; } = string.Empty;
        public string ResponsavelRelatorio { get; set; } = string.Empty;
        public string CargoResponsavel { get; set; } = string.Empty;
        public string HashRelatorio { get; set; } = string.Empty;
        public string AssinaturaDigitalRelatorio { get; set; } = string.Empty;
        public string TextoAutenticacao { get; set; } = string.Empty;
        public string? CartorioResponsavel { get; set; }
        public string? EnderecoCartorio { get; set; }
        
        // Dados para Validação Externa
        public string NumeroProtocolo { get; set; } = string.Empty;
        public string ChaveValidacao { get; set; } = string.Empty;
        public string URLValidacao { get; set; } = string.Empty;
    }

    // Request para gerar relatório cartorial
    public class RelatorioCartorialRequest
    {
        public Guid EleicaoId { get; set; }
        public bool IncluirDadosVotantes { get; set; } = true;
        public bool IncluirDadosTecnicos { get; set; } = true;
        public bool GerarAssinaturaDigital { get; set; } = true;
        public string? CartorioDestino { get; set; }
        public string? Observacoes { get; set; }
    }
}