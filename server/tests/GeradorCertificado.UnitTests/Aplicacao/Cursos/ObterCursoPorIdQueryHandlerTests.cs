using GeradorCertificado.Aplicacao.Modulos.Cursos;
using GeradorCertificado.Dominio.Modulos.Cursos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GeradorCertificado.UnitTests.Aplicacao.Cursos;

[TestClass]
public sealed class ObterCursoPorIdQueryHandlerTests
{
    private readonly Mock<IRepositorioCurso> repositorioCurso = new();

    private ObterCursoPorIdQueryHandler CriarHandler() => new(repositorioCurso.Object);

    [TestMethod]
    public async Task Handle_DeveRetornarCurso_QuandoEncontrado()
    {
        var curso = new Curso(Guid.CreateVersion7(), "Curso", "descrição", 40, DateTime.Today);
        repositorioCurso
            .Setup(repo => repo.SelecionarPorIdAsync(curso.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(curso);

        var handler = CriarHandler();
        var resultado = await handler.Handle(new ObterCursoPorIdQuery(curso.Id), CancellationToken.None);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(curso.Nome, resultado.Value.Nome);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoCursoNaoEncontrado()
    {
        repositorioCurso
            .Setup(repo => repo.SelecionarPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Curso?)null);

        var handler = CriarHandler();
        var resultado = await handler.Handle(new ObterCursoPorIdQuery(Guid.CreateVersion7()), CancellationToken.None);

        Assert.IsTrue(resultado.IsFailed);
    }
}
