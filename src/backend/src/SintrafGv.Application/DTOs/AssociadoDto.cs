namespace SintrafGv.Application.DTOs;

public record AssociadoDto(
    Guid Id,
    string Nome,
    string Cpf,
    string? MatriculaSindicato,
    string? MatriculaBancaria,
    // Dados pessoais
    string? Sexo,
    string? EstadoCivil,
    DateTime? DataNascimento,
    string? Naturalidade,
    // Endereço
    string? Cep,
    string? Endereco,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    // Dados bancários
    string? Banco,
    string? Agencia,
    string? CidadeAgencia,
    string? CodAgencia,
    string? Conta,
    // Dados profissionais
    string? Funcao,
    string? Ctps,
    string? Serie,
    // Datas importantes
    DateTime? DataAdmissao,
    DateTime? DataFiliacao,
    DateTime? DataDesligamento,
    // Contato
    string? Telefone,
    string? Celular,
    string? Email,
    // Status
    bool Filiado,
    bool Ativo,
    bool Encerrado,
    Guid? SubstituidoPorId,
    bool Aposentado,
    // Auditoria
    DateTime? DataUltimaAtualizacao,
    DateTime CriadoEm);

public record CreateAssociadoRequest(
    string Nome,
    string Cpf,
    string? MatriculaSindicato,
    string? MatriculaBancaria,
    // Dados pessoais
    string? Sexo,
    string? EstadoCivil,
    DateTime? DataNascimento,
    string? Naturalidade,
    // Endereço
    string? Cep,
    string? Endereco,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    // Dados bancários
    string? Banco,
    string? Agencia,
    string? CidadeAgencia,
    string? CodAgencia,
    string? Conta,
    // Dados profissionais
    string? Funcao,
    string? Ctps,
    string? Serie,
    // Datas importantes
    DateTime? DataAdmissao,
    DateTime? DataFiliacao,
    DateTime? DataDesligamento,
    // Contato
    string? Telefone,
    string? Celular,
    string? Email,
    // Status
    bool Filiado = true,
    bool Ativo = true,
    bool Aposentado = false);

public record UpdateAssociadoRequest(
    string Nome,
    string Cpf,
    string? MatriculaSindicato,
    string? MatriculaBancaria,
    // Dados pessoais
    string? Sexo,
    string? EstadoCivil,
    DateTime? DataNascimento,
    string? Naturalidade,
    // Endereço
    string? Cep,
    string? Endereco,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    // Dados bancários
    string? Banco,
    string? Agencia,
    string? CidadeAgencia,
    string? CodAgencia,
    string? Conta,
    // Dados profissionais
    string? Funcao,
    string? Ctps,
    string? Serie,
    // Datas importantes
    DateTime? DataAdmissao,
    DateTime? DataFiliacao,
    DateTime? DataDesligamento,
    // Contato
    string? Telefone,
    string? Celular,
    string? Email,
    // Status
    bool Filiado,
    bool Ativo,
    bool Aposentado);

public record TrocarBancoAssociadoRequest(
    string MatriculaBancaria,
    string Banco,
    string? MotivoEncerramento);
