using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Servicos;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GeradorCertificado.UnitTests.Aplicacao.GeracaoCertificados;

[TestClass]
public sealed class ObterZipCertificadosQueryHandlerTests
{
    private readonly Mock<IRepositorioSolicitacaoCertificado> repositorioSolicitacao = new();
    private readonly Mock<IArmazenamentoDeArquivos> armazenamento = new();

    private ObterZipCertificadosQueryHandler CriarHandler() =>
        new(repositorioSolicitacao.Object, armazenamento.Object);

    [TestMethod]
    public async Task Handle_DeveRetornarConteudoDoZip_QuandoSolicitacaoEstaConcluida()
    {
        var cursoId = Guid.CreateVersion7();
        var solicitacao = new SolicitacaoCertificado(Guid.CreateVersion7(), cursoId, []);
        solicitacao.Concluir("certificados/1/certificados.zip");

        repositorioSolicitacao
            .Setup(repo => repo.SelecionarMaisRecentePorCursoAsync(cursoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);

        byte[] conteudoEsperado = [1, 2, 3];
        armazenamento
            .Setup(a => a.LerAsync("certificados/1/certificados.zip", It.IsAny<CancellationToken>()))
            .ReturnsAsync(conteudoEsperado);

        var handler = CriarHandler();
        var resultado = await handler.Handle(new ObterZipCertificadosQuery(cursoId), CancellationToken.None);

        Assert.IsTrue(resultado.IsSuccess);
        CollectionAssert.AreEqual(conteudoEsperado, resultado.Value.Conteudo);
        Assert.AreEqual("application/zip", resultado.Value.ContentType);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoNenhumaSolicitacaoEncontrada()
    {
        repositorioSolicitacao
            .Setup(repo => repo.SelecionarMaisRecentePorCursoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SolicitacaoCertificado?)null);

        var handler = CriarHandler();
        var resultado = await handler.Handle(new ObterZipCertificadosQuery(Guid.CreateVersion7()), CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoZipAindaNaoFoiGerado()
    {
        var cursoId = Guid.CreateVersion7();
        var solicitacao = new SolicitacaoCertificado(Guid.CreateVersion7(), cursoId, []);
        solicitacao.IniciarProcessamento();

        repositorioSolicitacao
            .Setup(repo => repo.SelecionarMaisRecentePorCursoAsync(cursoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);

        var handler = CriarHandler();
        var resultado = await handler.Handle(new ObterZipCertificadosQuery(cursoId), CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
    }
}
