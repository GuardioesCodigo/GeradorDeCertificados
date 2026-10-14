using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Mensageria;
using GeradorCertificado.Dominio.Modulos.Cursos;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GeradorCertificado.UnitTests.Aplicacao.GeracaoCertificados;

[TestClass]
public sealed class SolicitarGeracaoCertificadosCommandHandlerTests
{
    private readonly Mock<IRepositorioCurso> repositorioCurso = new();
    private readonly Mock<IRepositorioSolicitacaoCertificado> repositorioSolicitacao = new();
    private readonly Mock<IPublishEndpoint> publishEndpoint = new();

    private SolicitarGeracaoCertificadosCommandHandler CriarHandler() =>
        new(repositorioCurso.Object, repositorioSolicitacao.Object, publishEndpoint.Object);

    private static Curso CriarCurso(Guid id) =>
        new(id, "Curso", "descrição", 40, DateTime.Today.AddMonths(1));

    [TestMethod]
    public async Task Handle_DeveCadastrarSolicitacaoEPublicarMensagem_QuandoDadosSaoValidos()
    {
        var cursoId = Guid.CreateVersion7();

        repositorioCurso
            .Setup(repo => repo.SelecionarPorIdAsync(cursoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCurso(cursoId));

        repositorioSolicitacao
            .Setup(repo => repo.SelecionarEmProcessamentoPorCursoAsync(cursoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SolicitacaoCertificado?)null);

        var handler = CriarHandler();
        var command = new SolicitarGeracaoCertificadosCommand(cursoId, [new AlunoParaCertificadoCommand("Aluno 1")]);

        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(resultado.IsSuccess);
        repositorioSolicitacao.Verify(
            repo => repo.CadastrarAsync(It.IsAny<SolicitacaoCertificado>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        publishEndpoint.Verify(
            p => p.Publish(It.IsAny<GerarCertificadosMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoCursoNaoExiste()
    {
        repositorioCurso
            .Setup(repo => repo.SelecionarPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Curso?)null);

        var handler = CriarHandler();
        var command = new SolicitarGeracaoCertificadosCommand(
            Guid.CreateVersion7(), [new AlunoParaCertificadoCommand("Aluno 1")]);

        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
        repositorioSolicitacao.Verify(
            repo => repo.CadastrarAsync(It.IsAny<SolicitacaoCertificado>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoListaDeAlunosEhVazia()
    {
        var handler = CriarHandler();
        var command = new SolicitarGeracaoCertificadosCommand(Guid.CreateVersion7(), []);

        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoNomeDeAlunoEhVazio()
    {
        var handler = CriarHandler();
        var command = new SolicitarGeracaoCertificadosCommand(
            Guid.CreateVersion7(), [new AlunoParaCertificadoCommand(string.Empty)]);

        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoJaExisteProcessamentoEmAndamento()
    {
        var cursoId = Guid.CreateVersion7();
        var solicitacaoExistente = new SolicitacaoCertificado(
            Guid.CreateVersion7(),
            cursoId,
            []
        );

        repositorioCurso
            .Setup(repo => repo.SelecionarPorIdAsync(cursoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CriarCurso(cursoId));

        repositorioSolicitacao
            .Setup(repo => repo.SelecionarEmProcessamentoPorCursoAsync(cursoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacaoExistente);

        var handler = CriarHandler();
        var command = new SolicitarGeracaoCertificadosCommand(cursoId, [new AlunoParaCertificadoCommand("Aluno 1")]);

        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
        repositorioSolicitacao.Verify(
            repo => repo.CadastrarAsync(It.IsAny<SolicitacaoCertificado>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
