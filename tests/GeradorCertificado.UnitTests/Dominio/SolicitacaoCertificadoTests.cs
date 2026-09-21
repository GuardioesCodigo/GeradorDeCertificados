using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.UnitTests.Dominio;

[TestClass]
public sealed class SolicitacaoCertificadoTests
{
    private static Certificado CriarCertificado(Guid solicitacaoId)
    {
        return new Certificado(Guid.CreateVersion7(), "Aluno", StatusCertificado.Pendente, null, null, solicitacaoId);
    }

    [TestMethod]
    public void Construtor_DeveIniciarComStatusPendente()
    {
        var solicitacaoId = Guid.CreateVersion7();
        var solicitacao = new SolicitacaoCertificado(solicitacaoId, Guid.CreateVersion7(), [CriarCertificado(solicitacaoId)]);

        Assert.AreEqual(StatusSolicitacao.Pendente, solicitacao.StatusSolicitacao);
    }

    [TestMethod]
    public void Validar_DeveRetornarErro_QuandoCursoIdEhVazio()
    {
        var solicitacao = new SolicitacaoCertificado(Guid.CreateVersion7(), Guid.Empty, [CriarCertificado(Guid.CreateVersion7())]);

        var erros = solicitacao.Validar();

        Assert.IsTrue(erros.Any(erro => erro.Campo == nameof(SolicitacaoCertificado.CursoId)));
    }

    [TestMethod]
    public void Validar_DeveRetornarErro_QuandoNaoHaCertificados()
    {
        var solicitacao = new SolicitacaoCertificado(Guid.CreateVersion7(), Guid.CreateVersion7(), []);

        var erros = solicitacao.Validar();

        Assert.IsTrue(erros.Any(erro => erro.Campo == nameof(SolicitacaoCertificado.Certificados)));
    }

    [TestMethod]
    public void IniciarProcessamento_DeveAlterarStatusParaGerandoCertificados()
    {
        var solicitacaoId = Guid.CreateVersion7();
        var solicitacao = new SolicitacaoCertificado(solicitacaoId, Guid.CreateVersion7(), [CriarCertificado(solicitacaoId)]);

        solicitacao.IniciarProcessamento();

        Assert.AreEqual(StatusSolicitacao.GerandoCertificados, solicitacao.StatusSolicitacao);
    }

    [TestMethod]
    public void IniciarGeracaoZip_DeveAlterarStatusParaGerandoZip()
    {
        var solicitacaoId = Guid.CreateVersion7();
        var solicitacao = new SolicitacaoCertificado(solicitacaoId, Guid.CreateVersion7(), [CriarCertificado(solicitacaoId)]);

        solicitacao.IniciarGeracaoZip();

        Assert.AreEqual(StatusSolicitacao.GerandoZip, solicitacao.StatusSolicitacao);
    }

    [TestMethod]
    public void Concluir_DeveAlterarStatusEGuardarCaminhoDoZip()
    {
        var solicitacaoId = Guid.CreateVersion7();
        var solicitacao = new SolicitacaoCertificado(solicitacaoId, Guid.CreateVersion7(), [CriarCertificado(solicitacaoId)]);

        solicitacao.Concluir("certificados/1/certificados.zip");

        Assert.AreEqual(StatusSolicitacao.Concluido, solicitacao.StatusSolicitacao);
        Assert.AreEqual("certificados/1/certificados.zip", solicitacao.CaminhoZip);
    }

    [TestMethod]
    public void Falhar_DeveAlterarStatusParaFalha()
    {
        var solicitacaoId = Guid.CreateVersion7();
        var solicitacao = new SolicitacaoCertificado(solicitacaoId, Guid.CreateVersion7(), [CriarCertificado(solicitacaoId)]);

        solicitacao.Falhar();

        Assert.AreEqual(StatusSolicitacao.Falha, solicitacao.StatusSolicitacao);
    }
}
