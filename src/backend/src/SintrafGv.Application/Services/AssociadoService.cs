using SintrafGv.Domain;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Domain.Entities;
using SintrafGv.Application.Exceptions;

namespace SintrafGv.Application.Services;

public class AssociadoService : IAssociadoService
{
    private readonly IAssociadoRepository _repository;

    public AssociadoService(IAssociadoRepository repository) => _repository = repository;

    public async Task<Associado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _repository.ObterPorIdAsync(id, cancellationToken);

    public async Task<(IReadOnlyList<Associado> Itens, int Total)> ListarAsync(int pagina, int porPagina, bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var skip = (pagina - 1) * porPagina;
        var itens = await _repository.ListarAsync(skip, porPagina, apenasAtivos, cancellationToken);
        var total = await _repository.ContarAsync(apenasAtivos, cancellationToken);
        return (itens, total);
    }

    public async Task<(IReadOnlyList<Associado> Itens, int Total)> ListarAsync(int pagina, int porPagina, string? busca, string? status, CancellationToken cancellationToken = default)
    {
        var skip = (pagina - 1) * porPagina;
        var s = status?.Trim();
        bool? statusAtivo = string.IsNullOrEmpty(s) ? null
            : s.Equals("Ativo", StringComparison.OrdinalIgnoreCase) ? true
            : s.Equals("Inativo", StringComparison.OrdinalIgnoreCase) ? false
            : null;
        var itens = await _repository.ListarAsync(skip, porPagina, busca, statusAtivo, cancellationToken);
        var total = await _repository.ContarAsync(busca, statusAtivo, cancellationToken);
        return (itens, total);
    }

    public async Task<IReadOnlyList<Associado>> ListarHistoricoPorCpfAsync(string cpf, CancellationToken cancellationToken = default) =>
        await _repository.ListarHistoricoPorCpfAsync(cpf, cancellationToken);

    public async Task<Associado> CriarAsync(Associado associado, CancellationToken cancellationToken = default)
    {
        associado.Cpf = DocumentoAssociado.NormalizarCpf(associado.Cpf);
        if (string.IsNullOrEmpty(associado.Cpf))
            throw new ArgumentException("CPF é obrigatório e deve conter apenas números.", nameof(associado));

        if (associado.Encerrado)
            throw new ArgumentException("Não é possível criar cadastro já encerrado.", nameof(associado));

        var ativoExistente = await _repository.ObterAtivoPorCpfAsync(associado.Cpf, cancellationToken);
        if (associado.Ativo && ativoExistente != null)
            throw new CpfDuplicadoException();

        if (associado.Ativo && !string.IsNullOrWhiteSpace(associado.MatriculaBancaria))
        {
            var matriculaEmUso = await _repository.ObterAtivoPorMatriculaBancariaAsync(
                associado.MatriculaBancaria, cancellationToken);
            if (matriculaEmUso != null)
                throw new ArgumentException("Matrícula bancária já está em uso por outro cadastro ativo.", nameof(associado));
        }

        associado.Id = Guid.NewGuid();
        associado.CriadoEm = DateTime.UtcNow;
        associado.Encerrado = false;
        associado.SubstituidoPorId = null;
        return await _repository.IncluirAsync(associado, cancellationToken);
    }

    public async Task AtualizarAsync(Associado associado, CancellationToken cancellationToken = default)
    {
        associado.Cpf = DocumentoAssociado.NormalizarCpf(associado.Cpf);
        if (string.IsNullOrEmpty(associado.Cpf))
            throw new ArgumentException("CPF é obrigatório e deve conter apenas números.", nameof(associado));

        var noBanco = await _repository.ObterPorIdAsync(associado.Id, cancellationToken);
        if (noBanco is null)
            throw new ArgumentException("Associado não encontrado.", nameof(associado));

        if (noBanco.Encerrado)
            throw new ArgumentException("Cadastro encerrado não pode ser alterado.", nameof(associado));

        if (associado.Ativo)
        {
            var outroAtivo = await _repository.ObterAtivoPorCpfAsync(associado.Cpf, cancellationToken);
            if (outroAtivo != null && outroAtivo.Id != associado.Id)
                throw new CpfDuplicadoException();
        }

        if (associado.Ativo && !string.IsNullOrWhiteSpace(associado.MatriculaBancaria))
        {
            var matriculaEmUso = await _repository.ObterAtivoPorMatriculaBancariaAsync(
                associado.MatriculaBancaria, cancellationToken);
            if (matriculaEmUso != null && matriculaEmUso.Id != associado.Id)
                throw new ArgumentException("Matrícula bancária já está em uso por outro cadastro ativo.", nameof(associado));
        }

        associado.Encerrado = noBanco.Encerrado;
        associado.SubstituidoPorId = noBanco.SubstituidoPorId;
        associado.DataUltimaAtualizacao = DateTime.UtcNow;
        await _repository.AtualizarAsync(associado, cancellationToken);
    }

    public async Task<Associado> TrocarBancoAsync(
        Guid associadoAtualId,
        string matriculaBancaria,
        string banco,
        string? motivoEncerramento,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(matriculaBancaria))
            throw new ArgumentException("Matrícula bancária é obrigatória.", nameof(matriculaBancaria));
        if (string.IsNullOrWhiteSpace(banco))
            throw new ArgumentException("Banco é obrigatório.", nameof(banco));

        var atual = await _repository.ObterPorIdAsync(associadoAtualId, cancellationToken);
        if (atual is null)
            throw new ArgumentException("Associado não encontrado.", nameof(associadoAtualId));
        if (atual.Encerrado)
            throw new ArgumentException("Este cadastro já está encerrado.", nameof(associadoAtualId));
        if (!DocumentoAssociado.EhCadastroAtual(atual))
            throw new ArgumentException("Somente o cadastro ativo pode ser transferido de banco.", nameof(associadoAtualId));

        var matriculaEmUso = await _repository.ObterAtivoPorMatriculaBancariaAsync(matriculaBancaria, cancellationToken);
        if (matriculaEmUso != null)
            throw new ArgumentException("Matrícula bancária já está em uso por outro cadastro ativo.", nameof(matriculaBancaria));

        var novo = new Associado
        {
            Id = Guid.NewGuid(),
            Nome = atual.Nome,
            Cpf = atual.Cpf,
            MatriculaSindicato = atual.MatriculaSindicato,
            MatriculaBancaria = matriculaBancaria.Trim(),
            Sexo = atual.Sexo,
            EstadoCivil = atual.EstadoCivil,
            DataNascimento = atual.DataNascimento,
            Naturalidade = atual.Naturalidade,
            Cep = atual.Cep,
            Endereco = atual.Endereco,
            Complemento = atual.Complemento,
            Bairro = atual.Bairro,
            Cidade = atual.Cidade,
            Estado = atual.Estado,
            Banco = banco.Trim(),
            Agencia = atual.Agencia,
            CidadeAgencia = atual.CidadeAgencia,
            CodAgencia = atual.CodAgencia,
            Conta = atual.Conta,
            Funcao = atual.Funcao,
            Ctps = atual.Ctps,
            Serie = atual.Serie,
            Carteirinha = atual.Carteirinha,
            Base = atual.Base,
            DataAdmissao = DateTime.UtcNow,
            DataFiliacao = atual.DataFiliacao,
            Telefone = atual.Telefone,
            Celular = atual.Celular,
            Email = atual.Email,
            Filiado = atual.Filiado,
            Ativo = true,
            Encerrado = false,
            Aposentado = atual.Aposentado,
            CriadoEm = DateTime.UtcNow,
        };

        atual.Ativo = false;
        atual.Encerrado = true;
        atual.DataDesligamento = DateTime.UtcNow;
        atual.Motivo = string.IsNullOrWhiteSpace(motivoEncerramento)
            ? $"Troca de banco ({atual.Banco} → {banco.Trim()})"
            : motivoEncerramento.Trim();
        atual.SubstituidoPorId = novo.Id;
        atual.DataUltimaAtualizacao = DateTime.UtcNow;

        await _repository.AtualizarAsync(atual, cancellationToken);
        return await _repository.IncluirAsync(novo, cancellationToken);
    }
}
