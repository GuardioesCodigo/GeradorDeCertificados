using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GeradorCertificado.UnitTests.Aplicacao.GeracaoCertificados;

[TestClass]
public sealed class ListarCertificadosQueryHandlerTests
{
    private readonly Mock<IRepositorioSolicitacaoCertificado> repositorioSolicitacao = new();

    private ListarCertificadosQueryHandler CriarHandler() => new(repositorioSolicitacao.Object);

    [TestMethod]
    public async Task Handle_DeveListarTodosOsCertificadosDaSolicitacaoMaisRecente()
    {
        var cursoId = Guid.CreateVersion7();
        var solicitacaoId = Guid.CreateVersion7();

        var certificados = new List<Certificado>
        {
            new(Guid.CreateVersion7(), "Aluno 1", StatusCertificado.Gerado, "caminho1.pdf", DateTime.Today, solicitacaoId),
            new(Guid.CreateVersion7(), "Aluno 2", StatusCertificado.Pendente, null, null, solicitacaoId),
        };

        repositorioSolicitacao
            .Setup(repo => repo.SelecionarMaisRecentePorCursoAsync(cursoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SolicitacaoCertificado(solicitacaoId, cursoId, certificados));

        var handler = CriarHandler();
        var resultado = await handler.Handle(new ListarCertificadosQuery(cursoId), CancellationToken.None);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(2, resultado.Value.Count);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoNenhumaSolicitacaoEncontrada()
    {
        repositorioSolicitacao
            .Setup(repo => repo.SelecionarMaisRecentePorCursoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SolicitacaoCertificado?)null);

        var handler = CriarHandler();
        var resultado = await handler.Handle(new ListarCertificadosQuery(Guid.CreateVersion7()), CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
    }
}
