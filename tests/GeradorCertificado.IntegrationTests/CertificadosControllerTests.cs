using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.IntegrationTests;

[TestClass]
public sealed class CertificadosControllerTests : BaseIntegrationTest
{
    [TestMethod]
    public async Task SolicitarGeracao_DeveRetornar202_QuandoDadosSaoValidos()
    {
        var cliente = await CriarClienteAutenticadoAsync();
        var cursoId = await CriarCursoAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync($"/cursos/{cursoId}/certificados", new
        {
            Alunos = new[] { new { Nome = "Aluno 1" }, new { Nome = "Aluno 2" } }
        });

        Assert.AreEqual(HttpStatusCode.Accepted, resposta.StatusCode);
    }

    [TestMethod]
    public async Task SolicitarGeracao_DeveRetornar404_QuandoCursoNaoExiste()
    {
        var cliente = await CriarClienteAutenticadoAsync();

        var resposta = await cliente.PostAsJsonAsync($"/cursos/{Guid.NewGuid()}/certificados", new
        {
            Alunos = new[] { new { Nome = "Aluno 1" } }
        });

        Assert.AreEqual(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [TestMethod]
    public async Task SolicitarGeracao_DeveRetornar400_QuandoListaDeAlunosEhVazia()
    {
        var cliente = await CriarClienteAutenticadoAsync();
        var cursoId = await CriarCursoAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync($"/cursos/{cursoId}/certificados", new
        {
            Alunos = Array.Empty<object>()
        });

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task SolicitarGeracao_DeveRetornar409_QuandoJaExisteProcessamentoEmAndamentoParaOCurso()
    {
        var cliente = await CriarClienteAutenticadoAsync();
        var cursoId = await CriarCursoAsync(cliente);

        var primeiraResposta = await cliente.PostAsJsonAsync($"/cursos/{cursoId}/certificados", new
        {
            Alunos = new[] { new { Nome = "Aluno 1" } }
        });
        Assert.AreEqual(HttpStatusCode.Accepted, primeiraResposta.StatusCode);

        var segundaResposta = await cliente.PostAsJsonAsync($"/cursos/{cursoId}/certificados", new
        {
            Alunos = new[] { new { Nome = "Aluno 2" } }
        });

        Assert.AreEqual(HttpStatusCode.Conflict, segundaResposta.StatusCode);
    }

    [TestMethod]
    public async Task ObterStatus_DeveRetornar404_QuandoNenhumaSolicitacaoFoiFeitaParaOCurso()
    {
        var cliente = await CriarClienteAutenticadoAsync();
        var cursoId = await CriarCursoAsync(cliente);

        var resposta = await cliente.GetAsync($"/cursos/{cursoId}/status");

        Assert.AreEqual(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [TestMethod]
    public async Task ListarCertificados_DeveRetornar404_QuandoNenhumaSolicitacaoFoiFeitaParaOCurso()
    {
        var cliente = await CriarClienteAutenticadoAsync();
        var cursoId = await CriarCursoAsync(cliente);

        var resposta = await cliente.GetAsync($"/cursos/{cursoId}/certificados");

        Assert.AreEqual(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Download_DeveRetornar401_QuandoNaoAutenticado()
    {
        var resposta = await Client.GetAsync($"/cursos/{Guid.NewGuid()}/certificados/download");

        Assert.AreEqual(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}
