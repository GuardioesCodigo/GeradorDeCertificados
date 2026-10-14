using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.IntegrationTests;

[TestClass]
public sealed class CursosControllerTests : BaseIntegrationTest
{
    [TestMethod]
    public async Task Cadastrar_DeveRetornar201_QuandoDadosSaoValidos()
    {
        var cliente = await CriarClienteAutenticadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/cursos", new
        {
            Nome = "Clean Architecture na prática",
            Descricao = "Curso sobre arquitetura em camadas.",
            CargaHoraria = 20,
            DataConclusao = DateTime.Today.AddDays(10)
        });

        Assert.AreEqual(HttpStatusCode.Created, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Cadastrar_DeveRetornar401_QuandoNaoAutenticado()
    {
        var resposta = await Client.PostAsJsonAsync("/cursos", new
        {
            Nome = "Curso sem token",
            Descricao = "descrição",
            CargaHoraria = 10,
            DataConclusao = DateTime.Today.AddDays(5)
        });

        Assert.AreEqual(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Cadastrar_DeveRetornar400_QuandoNomeEhVazio()
    {
        var cliente = await CriarClienteAutenticadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/cursos", new
        {
            Nome = "",
            Descricao = "descrição",
            CargaHoraria = 10,
            DataConclusao = DateTime.Today.AddDays(5)
        });

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Cadastrar_DeveRetornar400_QuandoCargaHorariaEhZero()
    {
        var cliente = await CriarClienteAutenticadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/cursos", new
        {
            Nome = "Curso",
            Descricao = "descrição",
            CargaHoraria = 0,
            DataConclusao = DateTime.Today.AddDays(5)
        });

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task ObterPorId_DeveRetornarCurso_QuandoExiste()
    {
        var cliente = await CriarClienteAutenticadoAsync();
        var cursoId = await CriarCursoAsync(cliente, "Curso para consulta");

        var resposta = await cliente.GetAsync($"/cursos/{cursoId}");

        Assert.AreEqual(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("Curso para consulta", corpo.GetProperty("nome").GetString());
    }

    [TestMethod]
    public async Task ObterPorId_DeveRetornar404_QuandoCursoNaoExiste()
    {
        var cliente = await CriarClienteAutenticadoAsync();

        var resposta = await cliente.GetAsync($"/cursos/{Guid.NewGuid()}");

        Assert.AreEqual(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [TestMethod]
    public async Task ObterPorId_DeveRetornar401_QuandoNaoAutenticado()
    {
        var resposta = await Client.GetAsync($"/cursos/{Guid.NewGuid()}");

        Assert.AreEqual(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}
