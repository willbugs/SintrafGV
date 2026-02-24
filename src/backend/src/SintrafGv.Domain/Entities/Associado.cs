namespace SintrafGv.Domain.Entities;

/// <summary>
/// Associado do sindicato SintrafGV (cadastro de pessoas).
/// Alinhado ao legado CPessoas com todos os campos.
/// </summary>
public class Associado
{
    public Guid Id { get; set; }

    // Dados básicos
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? MatriculaSindicato { get; set; }
    public string? MatriculaBancaria { get; set; }

    // Dados pessoais
    public string? Sexo { get; set; }
    public string? EstadoCivil { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Naturalidade { get; set; }

    // Endereço
    public string? Cep { get; set; }
    public string? Endereco { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }

    // Dados bancários
    public string? Banco { get; set; }
    public string? Agencia { get; set; }
    public string? CodAgencia { get; set; }
    public string? Conta { get; set; }

    // Dados profissionais
    public string? Funcao { get; set; }
    public string? Ctps { get; set; }
    public string? Serie { get; set; }

    // Datas importantes
    public DateTime? DataAdmissao { get; set; }
    public DateTime? DataFiliacao { get; set; }
    public DateTime? DataDesligamento { get; set; }

    // Contato
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }

    // Status
    /// <summary>Cadastro ativo no sistema (pode votar, receber comunicações)</summary>
    public bool Ativo { get; set; } = true;
    /// <summary>Status funcional: false = na ativa, true = aposentado do banco</summary>
    public bool Aposentado { get; set; } = false;

    // Auditoria
    public DateTime? DataUltimaAtualizacao { get; set; }
    public DateTime CriadoEm { get; set; }
}
