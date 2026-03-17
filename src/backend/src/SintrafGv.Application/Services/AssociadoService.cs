using SintrafGv.Domain.Interfaces;
using SintrafGv.Domain.Entities;
using SintrafGv.Application.Exceptions;

namespace SintrafGv.Application.Services;

public class AssociadoService : IAssociadoService
{
    private readonly IAssociadoRepository _repository;

    public AssociadoService(IAssociadoRepository repository) => _repository = repository;

    /// <summary>Normaliza CPF para apenas 11 dígitos (remove pontos, traços e espaços).</summary>
    private static string NormalizarCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return string.Empty;
        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        return digits.Length > 11 ? digits.Substring(0, 11) : digits;
    }

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

    public async Task<Associado> CriarAsync(Associado associado, CancellationToken cancellationToken = default)
    {
        associado.Cpf = NormalizarCpf(associado.Cpf);
        if (string.IsNullOrEmpty(associado.Cpf))
            throw new ArgumentException("CPF é obrigatório e deve conter apenas números.", nameof(associado));

        var existente = await _repository.ObterPorCpfAsync(associado.Cpf, cancellationToken);
        if (existente != null)
            throw new CpfDuplicadoException();

        associado.Id = Guid.NewGuid();
        associado.CriadoEm = DateTime.UtcNow;
        return await _repository.IncluirAsync(associado, cancellationToken);
    }

    public async Task AtualizarAsync(Associado associado, CancellationToken cancellationToken = default)
    {
        associado.Cpf = NormalizarCpf(associado.Cpf);
        if (string.IsNullOrEmpty(associado.Cpf))
            throw new ArgumentException("CPF é obrigatório e deve conter apenas números.", nameof(associado));

        var existente = await _repository.ObterPorCpfAsync(associado.Cpf, cancellationToken);
        if (existente != null && existente.Id != associado.Id)
            throw new CpfDuplicadoException();

        associado.DataUltimaAtualizacao = DateTime.UtcNow;
        await _repository.AtualizarAsync(associado, cancellationToken);
    }
}
