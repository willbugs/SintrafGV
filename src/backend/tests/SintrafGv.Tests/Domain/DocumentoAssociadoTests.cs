using SintrafGv.Domain;
using SintrafGv.Domain.Entities;
using Xunit;

namespace SintrafGv.Tests.Domain;

public class DocumentoAssociadoTests
{
    [Fact]
    public void EhCadastroAtual_ApenasQuandoAtivoENaoEncerrado()
    {
        var ativo = new Associado { Ativo = true, Encerrado = false };
        var encerrado = new Associado { Ativo = false, Encerrado = true };
        var inativo = new Associado { Ativo = false, Encerrado = false };

        Assert.True(DocumentoAssociado.EhCadastroAtual(ativo));
        Assert.False(DocumentoAssociado.EhCadastroAtual(encerrado));
        Assert.False(DocumentoAssociado.EhCadastroAtual(inativo));
    }

    [Fact]
    public void MatriculaCoincide_IgnoraZerosAEsquerda()
    {
        Assert.True(DocumentoAssociado.MatriculaCoincide("005717830", "5717830"));
        Assert.False(DocumentoAssociado.MatriculaCoincide("123", "456"));
    }

    [Fact]
    public void NormalizarCpf_RemoveFormatacao()
    {
        Assert.Equal("66984076668", DocumentoAssociado.NormalizarCpf("669.840.766-68"));
    }
}
