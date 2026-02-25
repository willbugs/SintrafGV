using System;
using System.ComponentModel.DataAnnotations;

namespace SintrafGv.Domain.Entities
{
    public class ConfiguracaoSindicato
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Razão Social é obrigatória")]
        [MaxLength(200)]
        public string RazaoSocial { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Nome Fantasia é obrigatório")]
        [MaxLength(200)]
        public string NomeFantasia { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "CNPJ é obrigatório")]
        [MaxLength(18)]
        public string CNPJ { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? InscricaoEstadual { get; set; }
        
        [Required(ErrorMessage = "Endereço é obrigatório")]
        [MaxLength(300)]
        public string Endereco { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Número é obrigatório")]
        [MaxLength(10)]
        public string Numero { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Complemento { get; set; }
        
        [Required(ErrorMessage = "Bairro é obrigatório")]
        [MaxLength(100)]
        public string Bairro { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Cidade é obrigatória")]
        [MaxLength(100)]
        public string Cidade { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "UF é obrigatória")]
        [MaxLength(2)]
        public string UF { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "CEP é obrigatório")]
        [MaxLength(9)]
        public string CEP { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? Telefone { get; set; }
        
        [MaxLength(20)]
        public string? Celular { get; set; }
        
        [MaxLength(200)]
        public string? Email { get; set; }
        
        [MaxLength(200)]
        public string? Website { get; set; }
        
        // Dados para Autenticação Cartorial
        [Required(ErrorMessage = "Presidente é obrigatório")]
        [MaxLength(200)]
        public string Presidente { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "CPF do Presidente é obrigatório")]
        [MaxLength(14)]
        public string CPFPresidente { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? Secretario { get; set; }
        
        [MaxLength(14)]
        public string? CPFSecretario { get; set; }
        
        // Dados para Relatórios Oficiais
        [MaxLength(500)]
        public string? TextoAutenticacao { get; set; }
        
        [MaxLength(200)]
        public string? CartorioResponsavel { get; set; }
        
        [MaxLength(300)]
        public string? EnderecoCartorio { get; set; }
        
        // Metadados
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }
        public string? CriadoPor { get; set; }
        public string? AtualizadoPor { get; set; }
    }
}