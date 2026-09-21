using GeradorCertificado.Aplicacao.Modulos.Cursos;
using GeradorCertificado.Dominio.Modulos.Cursos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GeradorCertificado.UnitTests.Aplicacao.Cursos;

[TestClass]
public sealed class CadastrarCursoCommandHandlerTests
{
    private readonly Mock<IRepositorioCurso> repositorioCurso = new();

    private CadastrarCursoCommandHandler CriarHandler() => new(repositorioCurso.Object);

    [TestMethod]
    public async Task Handle_DeveCadastrarECretornarSucesso_QuandoDadosSaoValidos()
    {
        var handler = CriarHandler();
        var command = new CadastrarCursoCommand("Curso de C#", "descrição", 40, DateTime.Today.AddMonths(1));

        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(resultado.IsSuccess);
        repositorioCurso.Verify(
            repo => repo.CadastrarAsync(It.IsAny<Curso>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [TestMethod]
    public async Task Handle_NaoDeveCadastrar_QuandoNomeEhVazio()
    {
        var handler = CriarHandler();
        var command = new CadastrarCursoCommand(string.Empty, "descrição", 40, DateTime.Today.AddMonths(1));

        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
        repositorioCurso.Verify(
            repo => repo.CadastrarAsync(It.IsAny<Curso>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoCargaHorariaEhZero()
    {
        var handler = CriarHandler();
        var command = new CadastrarCursoCommand("Curso", "descrição", 0, DateTime.Today.AddMonths(1));

        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoDataConclusaoNaoInformada()
    {
        var handler = CriarHandler();
        var command = new CadastrarCursoCommand("Curso", "descrição", 40, default);

        var resultado = await handler.Handle(command, CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
    }
}
