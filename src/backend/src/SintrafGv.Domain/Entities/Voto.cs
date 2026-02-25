using System.ComponentModel.DataAnnotations;

namespace SintrafGv.Domain.Entities;

/// <summary>
/// Registro de que um associado votou em uma eleição.
/// NÃO armazena em quem votou, apenas que participou.
/// Garante que cada associado vote apenas uma vez por eleição.
/// Inclui dados para autenticação cartorial.
/// </summary>
public class Voto
{
    public Guid Id { get; set; }
    
    public Guid EleicaoId { get; set; }
    public Eleicao? Eleicao { get; set; }
    
    public Guid AssociadoId { get; set; }
    public Associado? Associado { get; set; }
    
    // Dados Básicos do Voto
    public DateTime DataHoraVoto { get; set; }
    public string? IpOrigem { get; set; }
    public string? UserAgent { get; set; }
    public string? CodigoComprovante { get; set; }
    
    // Dados para Autenticação Cartorial
    [MaxLength(50)]
    public string TimestampPreciso { get; set; } = string.Empty; // Com milissegundos e timezone
    
    [MaxLength(100)]
    public string HashVoto { get; set; } = string.Empty; // Hash SHA-256 do voto + timestamp
    
    [MaxLength(500)]
    public string? AssinaturaDigital { get; set; } // Assinatura digital do hash
    
    // Dados da Resposta (Criptografados)
    [MaxLength(200)]
    public string RespostaCriptografada { get; set; } = string.Empty; // Resposta criptografada
    
    [MaxLength(100)]
    public string ChaveCriptografia { get; set; } = string.Empty; // Chave para descriptografar
    
    // Dados de Auditoria Adicional
    [MaxLength(50)]
    public string? NumeroSequencial { get; set; } // Número sequencial do voto na eleição
    
    [MaxLength(100)]
    public string? HashAnterior { get; set; } // Hash do voto anterior (blockchain-like)
    
    [MaxLength(200)]
    public string? DadosDispositivo { get; set; } // Informações do dispositivo usado
    
    [MaxLength(50)]
    public string? Latitude { get; set; } // Coordenada GPS (se autorizada)
    
    [MaxLength(50)]
    public string? Longitude { get; set; } // Coordenada GPS (se autorizada)
    
    // Metadados de Validação
    public bool ValidadoCartorio { get; set; } = false;
    public DateTime? DataValidacaoCartorio { get; set; }
    public string? NumeroProtocoloCartorio { get; set; }
    public string? ObservacoesValidacao { get; set; }
}
