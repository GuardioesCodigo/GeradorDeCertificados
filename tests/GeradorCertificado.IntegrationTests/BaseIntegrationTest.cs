using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.IntegrationTests;

[TestClass]
public abstract class BaseIntegrationTest
{
    protected CustomWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    [TestInitialize]
    public void ConfigurarTeste()
    {
        Factory = new CustomWebApplicationFactory();
        Client = Factory.CreateClient();
    }

    [TestCleanup]
    public void FinalizarTeste()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    protected static string GerarEmailUnico() => $"{Guid.CreateVersion7()}@teste.com";

    protected async Task<string> CadastrarEObterTokenAsync(string? email = null, string senha = "Senha123!")
    {
        email ??= GerarEmailUnico();

        var respostaCadastro = await Client.PostAsJsonAsync("/auth/cadastro", new { Email = email, Senha = senha });
        respostaCadastro.EnsureSuccessStatusCode();

        var respostaLogin = await Client.PostAsJsonAsync("/auth/login", new { Email = email, Senha = senha });
        respostaLogin.EnsureSuccessStatusCode();

        var corpo = await respostaLogin.Content.ReadFromJsonAsync<JsonElement>();

        return corpo.GetProperty("token").GetString()!;
    }

    protected async Task<HttpClient> CriarClienteAutenticadoAsync()
    {
        var token = await CadastrarEObterTokenAsync();
        var cliente = Factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return cliente;
    }

    protected static async Task<Guid> CriarCursoAsync(HttpClient clienteAutenticado, string? nome = null)
    {
        var resposta = await clienteAutenticado.PostAsJsonAsync("/cursos", new
        {
            Nome = nome ?? $"Curso {Guid.CreateVersion7()}",
            Descricao = "Descrição de curso usada nos testes automatizados.",
            CargaHoraria = 40,
            DataConclusao = DateTime.Today.AddDays(30)
        });

        resposta.EnsureSuccessStatusCode();

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        return corpo.GetProperty("id").GetGuid();
    }
}
