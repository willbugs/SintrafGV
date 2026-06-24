using SintrafGv.Domain.Entities;

namespace SintrafGv.Domain;

public static class DocumentoAssociado
{
    public static string NormalizarCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return string.Empty;
        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        return digits.Length > 11 ? digits[..11] : digits;
    }

    public static bool MatriculaCoincide(string? cadastrada, string? informada)
    {
        var db = (cadastrada ?? "").Trim();
        var input = (informada ?? "").Trim();
        if (string.IsNullOrEmpty(db) || string.IsNullOrEmpty(input)) return false;
        return db == input || db.TrimStart('0') == input.TrimStart('0');
    }

    public static bool EhCadastroAtual(Associado associado) =>
        associado.Ativo && !associado.Encerrado;
}
