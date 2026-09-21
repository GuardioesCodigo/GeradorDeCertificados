using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GeradorCertificado.UnitTests.Aplicacao.GeracaoCertificados;

[TestClass]
public sealed class ObterStatusSolicitacaoQueryHandlerTests
{
    private readonly Mock<IRepositorioSolicitacaoCertificado> repositorioSolicitacao = new();

    private ObterStatusSolicitacaoQueryHandler CriarHandler() => new(repositorioSolicitacao.Object);

    [TestMethod]
    public async Task Handle_DeveRetornarStatusComContagens_QuandoSolicitacaoExiste()
    {
        var cursoId = Guid.CreateVersion7();
        var solicitacaoId = Guid.CreateVersion7();

        var certificados = new List<Certificado>
        {
            new(Guid.CreateVersion7(), "Aluno 1", StatusCertificado.Gerado, "caminho1.pdf", DateTime.Today, solicitacaoId),
            new(Guid.CreateVersion7(), "Aluno 2", StatusCertificado.Falha, null, null, solicitacaoId),
        };

        var solicitacao = new SolicitacaoCertificado(solicitacaoId, cursoId, certificados);

        repositorioSolicitacao
            .Setup(repo => repo.SelecionarMaisRecentePorCursoAsync(cursoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);

        var handler = CriarHandler();
        var resultado = await handler.Handle(new ObterStatusSolicitacaoQuery(cursoId), CancellationToken.None);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(2, resultado.Value.TotalCertificados);
        Assert.AreEqual(1, resultado.Value.CertificadosGerados);
        Assert.AreEqual(1, resultado.Value.CertificadosComFalha);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoNenhumaSolicitacaoEncontrada()
    {
        repositorioSolicitacao
            .Setup(repo => repo.SelecionarMaisRecentePorCursoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SolicitacaoCertificado?)null);

        var handler = CriarHandler();
        var resultado = await handler.Handle(new ObterStatusSolicitacaoQuery(Guid.CreateVersion7()), CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
    }
}
