using Microsoft.EntityFrameworkCore;
using SintrafGv.Domain;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Domain.Entities;
using SintrafGv.Infrastructure.Data;

namespace SintrafGv.Infrastructure.Repositories;

public class AssociadoRepository : IAssociadoRepository
{
    private readonly AppDbContext _context;

    public AssociadoRepository(AppDbContext context) => _context = context;

    public async Task<Associado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Associados.FindAsync([id], cancellationToken);

    public async Task<Associado?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfDigits = DocumentoAssociado.NormalizarCpf(cpf);
        if (string.IsNullOrEmpty(cpfDigits)) return null;
        var associados = await ListarPorCpfNormalizadoAsync(cpfDigits, cancellationToken);
        return associados.FirstOrDefault();
    }

    public async Task<Associado?> ObterAtivoPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfDigits = DocumentoAssociado.NormalizarCpf(cpf);
        if (string.IsNullOrEmpty(cpfDigits)) return null;
        var associados = await ListarPorCpfNormalizadoAsync(cpfDigits, cancellationToken);
        return associados.FirstOrDefault(DocumentoAssociado.EhCadastroAtual);
    }

    public async Task<Associado?> ObterAtivoPorCpfEMatriculaAsync(
        string cpf,
        string matriculaBancaria,
        CancellationToken cancellationToken = default)
    {
        var cpfDigits = DocumentoAssociado.NormalizarCpf(cpf);
        if (string.IsNullOrEmpty(cpfDigits)) return null;
        var associados = await ListarPorCpfNormalizadoAsync(cpfDigits, cancellationToken);
        return associados.FirstOrDefault(a =>
            DocumentoAssociado.EhCadastroAtual(a) &&
            DocumentoAssociado.MatriculaCoincide(a.MatriculaBancaria, matriculaBancaria));
    }

    public async Task<Associado?> ObterAtivoPorMatriculaBancariaAsync(
        string matriculaBancaria,
        CancellationToken cancellationToken = default)
    {
        var matriculaDigits = new string(matriculaBancaria.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(matriculaDigits)) return null;

        var associados = await _context.Associados
            .AsNoTracking()
            .Where(a => a.Ativo && !a.Encerrado && a.MatriculaBancaria != null && a.MatriculaBancaria != "")
            .ToListAsync(cancellationToken);

        return associados.FirstOrDefault(a =>
        {
            var dbDigits = new string(a.MatriculaBancaria!.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(dbDigits)) return false;
            return dbDigits == matriculaDigits ||
                   dbDigits.TrimStart('0') == matriculaDigits.TrimStart('0');
        });
    }

    public async Task<IReadOnlyList<Associado>> ListarHistoricoPorCpfAsync(
        string cpf,
        CancellationToken cancellationToken = default)
    {
        var cpfDigits = DocumentoAssociado.NormalizarCpf(cpf);
        if (string.IsNullOrEmpty(cpfDigits)) return Array.Empty<Associado>();
        var associados = await ListarPorCpfNormalizadoAsync(cpfDigits, cancellationToken);
        return associados
            .OrderByDescending(DocumentoAssociado.EhCadastroAtual)
            .ThenByDescending(a => a.CriadoEm)
            .ToList();
    }

    public async Task<Associado?> ObterPorMatriculaBancariaAsync(string matriculaBancaria, CancellationToken cancellationToken = default)
    {
        var matriculaDigits = new string(matriculaBancaria.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(matriculaDigits)) return null;

        var associados = await _context.Associados
            .AsNoTracking()
            .Where(a => a.MatriculaBancaria != null && a.MatriculaBancaria != "")
            .ToListAsync(cancellationToken);

        return associados.FirstOrDefault(a =>
        {
            var dbDigits = new string(a.MatriculaBancaria!.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(dbDigits)) return false;
            return dbDigits == matriculaDigits ||
                   dbDigits.TrimStart('0') == matriculaDigits.TrimStart('0');
        });
    }

    private async Task<List<Associado>> ListarPorCpfNormalizadoAsync(string cpfDigits, CancellationToken cancellationToken)
    {
        var associados = await _context.Associados
            .FromSqlRaw(
                "SELECT * FROM Associados WHERE REPLACE(REPLACE(REPLACE(ISNULL(Cpf,''), '.', ''), '-', ''), ' ', '') = {0}",
                cpfDigits)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return associados;
    }

    public async Task<IReadOnlyList<Associado>> ListarAsync(int skip, int take, bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Associados.AsNoTracking();
        if (apenasAtivos)
            query = query.Where(x => x.Ativo && !x.Encerrado);
        return await query.OrderBy(x => x.Nome).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Associado>> ListarAsync(int skip, int take, string? busca, bool? statusAtivo, CancellationToken cancellationToken = default)
    {
        var query = AplicarFiltros(_context.Associados.AsNoTracking(), busca, statusAtivo);
        return await query.OrderBy(x => x.Nome).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Associados.AsNoTracking();
        if (apenasAtivos)
            query = query.Where(x => x.Ativo && !x.Encerrado);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(string? busca, bool? statusAtivo, CancellationToken cancellationToken = default)
    {
        var query = AplicarFiltros(_context.Associados.AsNoTracking(), busca, statusAtivo);
        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<Associado> AplicarFiltros(IQueryable<Associado> query, string? busca, bool? statusAtivo)
    {
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var cpfDigits = new string(termo.Where(char.IsDigit).ToArray());
            query = query.Where(a =>
                (a.Nome != null && a.Nome.Contains(termo)) ||
                (a.Cpf != null && (a.Cpf.Contains(termo) || (cpfDigits.Length >= 3 && a.Cpf.Contains(cpfDigits)))) ||
                (a.Email != null && a.Email.Contains(termo)));
        }
        if (statusAtivo.HasValue)
            query = statusAtivo.Value
                ? query.Where(x => x.Ativo && !x.Encerrado)
                : query.Where(x => !x.Ativo || x.Encerrado);
        return query;
    }

    public async Task<Associado> IncluirAsync(Associado associado, CancellationToken cancellationToken = default)
    {
        _context.Associados.Add(associado);
        await _context.SaveChangesAsync(cancellationToken);
        return associado;
    }

    public async Task<int> ContarAssociadosAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Associados
            .CountAsync(a => a.Ativo && !a.Encerrado, cancellationToken);
    }

    public async Task<List<Associado>> ObterPorIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Associados
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Associado associado, CancellationToken cancellationToken = default)
    {
        _context.Associados.Update(associado);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<string>> ObterCidadesDistintasAsync(CancellationToken cancellationToken = default)
    {
        var raw = await _context.Associados
            .AsNoTracking()
            .Select(a => a.Cidade)
            .Distinct()
            .ToListAsync(cancellationToken);

        var cidades = raw
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (raw.Any(c => string.IsNullOrWhiteSpace(c)))
            cidades.Add("Não informado");

        return cidades.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<List<string>> ObterBancosDistintosAsync(CancellationToken cancellationToken = default)
    {
        var raw = await _context.Associados
            .AsNoTracking()
            .Select(a => a.Banco)
            .Distinct()
            .ToListAsync(cancellationToken);

        return raw
            .Select(b => string.IsNullOrWhiteSpace(b) ? "Não informado" : b.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
