using SintrafGv.Application.Interfaces;
using SintrafGv.Domain.Entities;

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

    public async Task<Associado> CriarAsync(Associado associado, CancellationToken cancellationToken = default)
    {
        associado.Id = Guid.NewGuid();
        associado.CriadoEm = DateTime.UtcNow;
        return await _repository.IncluirAsync(associado, cancellationToken);
    }

    public async Task AtualizarAsync(Associado associado, CancellationToken cancellationToken = default)
    {
        associado.DataUltimaAtualizacao = DateTime.UtcNow;
        await _repository.AtualizarAsync(associado, cancellationToken);
    }
}
