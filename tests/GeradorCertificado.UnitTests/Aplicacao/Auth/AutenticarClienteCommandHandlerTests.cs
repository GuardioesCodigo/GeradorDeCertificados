using GeradorCertificado.Aplicacao.Modulos.Auth;
using GeradorCertificado.Dominio.Compartilhado.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GeradorCertificado.UnitTests.Aplicacao.Auth;

[TestClass]
public sealed class AutenticarClienteCommandHandlerTests
{
    private readonly Mock<IGerenciadorDeIdentidade> gerenciadorDeIdentidade = new();
    private readonly Mock<IEmissorDeTokens> emissorDeTokens = new();

    private AutenticarClienteCommandHandler CriarHandler() =>
        new(gerenciadorDeIdentidade.Object, emissorDeTokens.Object);

    [TestMethod]
    public async Task Handle_DeveRetornarToken_QuandoCredenciaisSaoValidas()
    {
        var usuarioId = Guid.CreateVersion7();
        var dataExpiracao = DateTime.UtcNow.AddHours(1);

        gerenciadorDeIdentidade
            .Setup(g => g.ChecarValidadeDeSenhaAsync("teste@exemplo.com", "Senha123!", TipoUsuario.Cliente))
            .ReturnsAsync(new UsuarioDto(usuarioId, "teste@exemplo.com"));

        emissorDeTokens
            .Setup(e => e.CriarToken(usuarioId, "teste@exemplo.com", TipoUsuario.Cliente))
            .Returns(new AccessToken("token-jwt-fake", dataExpiracao));

        var handler = CriarHandler();
        var resultado = await handler.Handle(
            new AutenticarClienteCommand("teste@exemplo.com", "Senha123!"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual("token-jwt-fake", resultado.Value.Token);
        Assert.AreEqual(usuarioId, resultado.Value.UsuarioId);
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoCredenciaisSaoInvalidas()
    {
        gerenciadorDeIdentidade
            .Setup(g => g.ChecarValidadeDeSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoUsuario>()))
            .ReturnsAsync((UsuarioDto?)null);

        var handler = CriarHandler();
        var resultado = await handler.Handle(
            new AutenticarClienteCommand("teste@exemplo.com", "SenhaErrada1!"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
        emissorDeTokens.Verify(
            e => e.CriarToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<TipoUsuario>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoEmailEhVazio()
    {
        var handler = CriarHandler();

        var resultado = await handler.Handle(
            new AutenticarClienteCommand(string.Empty, "Senha123!"),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
        gerenciadorDeIdentidade.Verify(
            g => g.ChecarValidadeDeSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoUsuario>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task Handle_DeveRetornarFalha_QuandoSenhaEhVazia()
    {
        var handler = CriarHandler();

        var resultado = await handler.Handle(
            new AutenticarClienteCommand("teste@exemplo.com", string.Empty),
            CancellationToken.None
        );

        Assert.IsTrue(resultado.IsFailed);
    }
}
