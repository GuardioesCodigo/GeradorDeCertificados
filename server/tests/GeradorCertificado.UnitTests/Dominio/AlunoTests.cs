using GeradorCertificado.Dominio.Modulos.Alunos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.UnitTests.Dominio;

[TestClass]
public sealed class AlunoTests
{
    [TestMethod]
    public void NormalizarNome_DeveRemoverEspacosExtras()
    {
        Assert.AreEqual("Maria da Silva", Aluno.NormalizarNome("  Maria   da  Silva "));
    }

    [TestMethod]
    public void NormalizarNome_DeveRetornarVazio_QuandoNomeEhNuloOuEmBranco()
    {
        Assert.AreEqual(string.Empty, Aluno.NormalizarNome(null));
        Assert.AreEqual(string.Empty, Aluno.NormalizarNome("   "));
    }

    [TestMethod]
    public void Validar_DeveRetornarErro_QuandoNomeEhVazio()
    {
        var aluno = new Aluno(Guid.CreateVersion7(), "");

        Assert.AreEqual(1, aluno.Validar().Count);
    }

    [TestMethod]
    public void Validar_DeveRetornarErro_QuandoNomeUltrapassa200Caracteres()
    {
        var aluno = new Aluno(Guid.CreateVersion7(), new string('a', 201));

        Assert.AreEqual(1, aluno.Validar().Count);
    }

    [TestMethod]
    public void Validar_NaoDeveRetornarErro_QuandoNomeEhValido()
    {
        var aluno = new Aluno(Guid.CreateVersion7(), "Maria da Silva");

        Assert.AreEqual(0, aluno.Validar().Count);
    }
}
