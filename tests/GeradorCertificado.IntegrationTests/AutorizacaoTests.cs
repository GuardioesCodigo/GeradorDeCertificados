using System.Net;
using System.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.IntegrationTests;

[TestClass]
public sealed class AutorizacaoTests : BaseIntegrationTest
{
    [TestMethod]
    public async Task RotaProtegida_DeveRetornar401_QuandoTokenEhInvalido()
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-invalido-e-mal-formado");

        var resposta = await Client.GetAsync($"/cursos/{Guid.NewGuid()}");

        Assert.AreEqual(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [TestMethod]
    public async Task RotaProtegida_DevePermitirAcesso_QuandoTokenEhValido()
    {
        var cliente = await CriarClienteAutenticadoAsync();

        var resposta = await cliente.GetAsync($"/cursos/{Guid.NewGuid()}");

        // 404 (curso não existe) e não 401 — prova que o token válido passou pela autenticação.
        Assert.AreEqual(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [TestMethod]
    public async Task ObterPorId_DeveRetornar401_QuandoOTokenNaoTemPrefixoBearer()
    {
        var token = await CadastrarEObterTokenAsync();
        Client.DefaultRequestHeaders.Add("Authorization", token); // sem o prefixo "Bearer "

        var resposta = await Client.GetAsync($"/cursos/{Guid.NewGuid()}");

        Assert.AreEqual(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}
