using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.IntegrationTests;

/// <summary>
/// Testes ponta a ponta que percorrem a jornada completa de um usuário real através da API HTTP,
/// incluindo o processamento assíncrono feito pelo consumer do MassTransit (aqui rodando com o
/// transporte in-memory, para não depender de um RabbitMQ real em CI). Cada teste passa por
/// cadastro -> login -> curso -> solicitação -> processamento -> download, exatamente como um
/// cliente real da API faria.
/// </summary>
[TestClass]
public sealed class FluxoCompletoE2ETests : BaseIntegrationTest
{
    private static readonly TimeSpan TempoLimiteDeProcessamento = TimeSpan.FromSeconds(15);

    [TestMethod]
    public async Task FluxoCompleto_DeveGerarEDisponibilizarOZipDeCertificados()
    {
        // 1. Cadastro e login
        var cliente = await CriarClienteAutenticadoAsync();

        // 2. Criação do curso
        var cursoId = await CriarCursoAsync(cliente, "Fundamentos de Clean Architecture");

        // 3. Solicitação de geração de certificados para dois alunos
        var respostaSolicitacao = await cliente.PostAsJsonAsync($"/cursos/{cursoId}/certificados", new
        {
            Alunos = new[] { new { Nome = "Ada Lovelace" }, new { Nome = "Alan Turing" } }
        });

        Assert.AreEqual(HttpStatusCode.Accepted, respostaSolicitacao.StatusCode);

        // 4. Aguarda o consumer processar a solicitação de forma assíncrona
        var status = await AguardarConclusaoDoProcessamentoAsync(cliente, cursoId);

        Assert.AreEqual("Concluido", status.GetProperty("status").GetString());
        Assert.AreEqual(2, status.GetProperty("totalCertificados").GetInt32());
        Assert.AreEqual(2, status.GetProperty("certificadosGerados").GetInt32());
        Assert.AreEqual(0, status.GetProperty("certificadosComFalha").GetInt32());

        // 5. Lista os certificados gerados
        var respostaListagem = await cliente.GetAsync($"/cursos/{cursoId}/certificados");
        Assert.AreEqual(HttpStatusCode.OK, respostaListagem.StatusCode);

        var certificados = await respostaListagem.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual(2, certificados.GetArrayLength());

        // 6. Baixa o ZIP e confere que ele realmente contém os dois PDFs
        var respostaDownload = await cliente.GetAsync($"/cursos/{cursoId}/certificados/download");
        Assert.AreEqual(HttpStatusCode.OK, respostaDownload.StatusCode);
        Assert.AreEqual("application/zip", respostaDownload.Content.Headers.ContentType?.MediaType);

        await using var conteudoZip = await respostaDownload.Content.ReadAsStreamAsync();
        using var arquivoZip = new ZipArchive(conteudoZip, ZipArchiveMode.Read);

        Assert.AreEqual(2, arquivoZip.Entries.Count);
        Assert.IsTrue(arquivoZip.Entries.All(entrada => entrada.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task FluxoCompleto_NaoDevePermitirNovaSolicitacaoEnquantoAAnteriorNaoTerminou()
    {
        var cliente = await CriarClienteAutenticadoAsync();
        var cursoId = await CriarCursoAsync(cliente);

        var primeira = await cliente.PostAsJsonAsync($"/cursos/{cursoId}/certificados", new
        {
            Alunos = new[] { new { Nome = "Aluno Único" } }
        });
        Assert.AreEqual(HttpStatusCode.Accepted, primeira.StatusCode);

        // Mesmo tentando de novo imediatamente (antes do processamento terminar), deve ser bloqueado.
        var segunda = await cliente.PostAsJsonAsync($"/cursos/{cursoId}/certificados", new
        {
            Alunos = new[] { new { Nome = "Outro Aluno" } }
        });
        Assert.AreEqual(HttpStatusCode.Conflict, segunda.StatusCode);

        // Depois que a primeira solicitação termina, uma nova já é aceita normalmente.
        await AguardarConclusaoDoProcessamentoAsync(cliente, cursoId);

        var terceira = await cliente.PostAsJsonAsync($"/cursos/{cursoId}/certificados", new
        {
            Alunos = new[] { new { Nome = "Novo Aluno" } }
        });
        Assert.AreEqual(HttpStatusCode.Accepted, terceira.StatusCode);
    }

    private static async Task<JsonElement> AguardarConclusaoDoProcessamentoAsync(HttpClient cliente, Guid cursoId)
    {
        var inicio = DateTime.UtcNow;

        while (DateTime.UtcNow - inicio < TempoLimiteDeProcessamento)
        {
            var resposta = await cliente.GetAsync($"/cursos/{cursoId}/status");
            resposta.EnsureSuccessStatusCode();

            var status = await resposta.Content.ReadFromJsonAsync<JsonElement>();
            var statusAtual = status.GetProperty("status").GetString();

            if (statusAtual is "Concluido" or "Falha")
                return status;

            await Task.Delay(200);
        }

        Assert.Fail($"O processamento da solicitação do curso {cursoId} não terminou em {TempoLimiteDeProcessamento.TotalSeconds}s.");

        throw new InvalidOperationException("Inalcançável.");
    }
}
