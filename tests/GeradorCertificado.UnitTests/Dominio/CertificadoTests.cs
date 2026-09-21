using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.UnitTests.Dominio;

[TestClass]
public sealed class CertificadoTests
{
    private static Certificado CriarCertificadoPendente()
    {
        return new Certificado(
            Guid.CreateVersion7(),
            "Aluno Teste",
            StatusCertificado.Pendente,
            caminhoArquivo: null,
            dataGeracao: null,
            solicitacaoCertificadoId: Guid.CreateVersion7()
        );
    }

    [TestMethod]
    public void Validar_DeveRetornarSemErros_QuandoNomeAlunoEhValido()
    {
        var certificado = CriarCertificadoPendente();

        var erros = certificado.Validar();

        Assert.AreEqual(0, erros.Count);
    }

    [TestMethod]
    public void Validar_DeveRetornarErro_QuandoNomeAlunoEhVazio()
    {
        var certificado = new Certificado(
            Guid.CreateVersion7(), string.Empty, StatusCertificado.Pendente, null, null, Guid.CreateVersion7());

        var erros = certificado.Validar();

        Assert.IsTrue(erros.Any(erro => erro.Campo == nameof(Certificado.NomeAluno)));
    }

    [TestMethod]
    public void Validar_DeveRetornarErro_QuandoNomeAlunoExcede200Caracteres()
    {
        var certificado = new Certificado(
            Guid.CreateVersion7(), new string('A', 201), StatusCertificado.Pendente, null, null, Guid.CreateVersion7());

        var erros = certificado.Validar();

        Assert.IsTrue(erros.Any(erro => erro.Campo == nameof(Certificado.NomeAluno)));
    }

    [TestMethod]
    public void MarcarComoGerado_DeveAtualizarStatusCaminhoEData()
    {
        var certificado = CriarCertificadoPendente();
        var dataGeracao = new DateTime(2026, 9, 21);

        certificado.MarcarComoGerado("certificados/1/1.pdf", dataGeracao);

        Assert.AreEqual(StatusCertificado.Gerado, certificado.Status);
        Assert.AreEqual("certificados/1/1.pdf", certificado.CaminhoArquivo);
        Assert.AreEqual(dataGeracao, certificado.DataGeracao);
    }

    [TestMethod]
    public void MarcarComoFalha_DeveLimparCaminhoEDataDeGeracao()
    {
        var certificado = CriarCertificadoPendente();
        certificado.MarcarComoGerado("certificados/1/1.pdf", DateTime.Today);

        certificado.MarcarComoFalha();

        Assert.AreEqual(StatusCertificado.Falha, certificado.Status);
        Assert.IsNull(certificado.CaminhoArquivo);
        Assert.IsNull(certificado.DataGeracao);
    }
}
