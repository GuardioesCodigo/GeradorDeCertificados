using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.IntegrationTests;

[TestClass]
public sealed class AuthControllerTests : BaseIntegrationTest
{
    [TestMethod]
    public async Task Cadastro_DeveRetornar201_QuandoDadosSaoValidos()
    {
        var resposta = await Client.PostAsJsonAsync("/auth/cadastro", new
        {
            Email = GerarEmailUnico(),
            Senha = "Senha123!"
        });

        Assert.AreEqual(HttpStatusCode.Created, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Cadastro_DeveRetornar409_QuandoEmailJaCadastrado()
    {
        var email = GerarEmailUnico();

        var primeiraResposta = await Client.PostAsJsonAsync("/auth/cadastro", new { Email = email, Senha = "Senha123!" });
        Assert.AreEqual(HttpStatusCode.Created, primeiraResposta.StatusCode);

        var segundaResposta = await Client.PostAsJsonAsync("/auth/cadastro", new { Email = email, Senha = "OutraSenha1!" });

        Assert.AreEqual(HttpStatusCode.Conflict, segundaResposta.StatusCode);
    }

    [TestMethod]
    public async Task Cadastro_DeveRetornar400_QuandoSenhaNaoAtendeARegraDeNegocio()
    {
        var resposta = await Client.PostAsJsonAsync("/auth/cadastro", new
        {
            Email = GerarEmailUnico(),
            Senha = "semdigito" // sem dígito e sem caractere não alfanumérico
        });

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Cadastro_DeveRetornar400_QuandoEmailEhInvalido()
    {
        var resposta = await Client.PostAsJsonAsync("/auth/cadastro", new
        {
            Email = "isso-nao-e-um-email",
            Senha = "Senha123!"
        });

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Login_DeveRetornarToken_QuandoCredenciaisSaoValidas()
    {
        var email = GerarEmailUnico();
        await Client.PostAsJsonAsync("/auth/cadastro", new { Email = email, Senha = "Senha123!" });

        var resposta = await Client.PostAsJsonAsync("/auth/login", new { Email = email, Senha = "Senha123!" });

        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.IsFalse(string.IsNullOrWhiteSpace(corpo.GetProperty("token").GetString()));
    }

    [TestMethod]
    public async Task Login_DeveRetornar401_QuandoSenhaEstaErrada()
    {
        var email = GerarEmailUnico();
        await Client.PostAsJsonAsync("/auth/cadastro", new { Email = email, Senha = "Senha123!" });

        var resposta = await Client.PostAsJsonAsync("/auth/login", new { Email = email, Senha = "SenhaErrada9!" });

        Assert.AreEqual(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Login_DeveRetornar401_QuandoUsuarioNaoExiste()
    {
        var resposta = await Client.PostAsJsonAsync("/auth/login", new
        {
            Email = GerarEmailUnico(),
            Senha = "Senha123!"
        });

        Assert.AreEqual(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}
