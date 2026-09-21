using GeradorCertificado.Aplicacao.Modulos.Auth;
using GeradorCertificado.Dominio.Compartilhado.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GeradorCertificado.UnitTests.Aplicacao.Auth;

[TestClass]
public sealed class CadastrarClienteCommandHandlerTests
{
    private readonly Mock<IGerenciadorDeIdentidade> gerenciadorDeIdentidade = new();

    private CadastrarClienteCommandHandler CriarHandler() => new(gerenciadorDeIdentidade.Object);

    [TestMethod]
    public async Task Handle_DeveCadastrarERetornarId_QuandoDadosSaoValidos()
    {
        var usuarioId = Guid.CreateVersion7();
        gerenciadorDeIdentidade
            .Setup(g => g.CadastrarAsync(It.IsAny<Guid>(), "teste@exemplo.com", "Senha123!", TipoUsuario.Cliente))
            .ReturnsAsync(new UsuarioDto(usuarioId, "teste@exemplo.com"));

        var handler = CriarHandler();
        var resultado = await handler.Handle(
            new CadastrarClienteCommand("teste@exemplo.com", "Senha123!"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(usuarioId, resultado.Value);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoEmailEhInvalido()
    {
        var handler = CriarHandler();

        var resultado = await handler.Handle(
            new CadastrarClienteCommand("nao-e-um-email", "Senha123!"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
        gerenciadorDeIdentidade.Verify(
            g => g.CadastrarAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoUsuario>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoSenhaEhMenorQue8Caracteres()
    {
        var handler = CriarHandler();

        var resultado = await handler.Handle(
            new CadastrarClienteCommand("teste@exemplo.com", "Ab1!"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoSenhaNaoTemDigito()
    {
        var handler = CriarHandler();

        var resultado = await handler.Handle(
            new CadastrarClienteCommand("teste@exemplo.com", "SenhaSemDigito!"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoSenhaNaoTemCaractereNaoAlfanumerico()
    {
        var handler = CriarHandler();

        var resultado = await handler.Handle(
            new CadastrarClienteCommand("teste@exemplo.com", "Senha12345"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoEmailJaCadastrado()
    {
        gerenciadorDeIdentidade
            .Setup(g => g.CadastrarAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoUsuario>()))
            .ThrowsAsync(new ConflitoDeIdentidadeException("Já existe um usuário cadastrado com este email."));

        var handler = CriarHandler();
        var resultado = await handler.Handle(
            new CadastrarClienteCommand("duplicado@exemplo.com", "Senha123!"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
    }
}
