namespace SintrafGv.Application.Exceptions;

/// <summary>
/// Lançada quando se tenta cadastrar ou atualizar um associado com CPF já existente para outro cadastro.
/// </summary>
public class CpfDuplicadoException : InvalidOperationException
{
    public CpfDuplicadoException()
        : base("Já existe um associado cadastrado com este CPF.")
    {
    }

    public CpfDuplicadoException(string message) : base(message) { }
}
