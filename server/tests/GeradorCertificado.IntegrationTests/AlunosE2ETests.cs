using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.IntegrationTests;

/// <summary>
/// O nome do aluno fica gravado: quem recebe certificados em cursos diferentes é o mesmo aluno
/// e pode baixar um certificado (PDF) ou vários, de vários cursos, em um único ZIP.
/// </summary>
[TestClass]
public sealed class AlunosE2ETests : BaseIntegrationTest
{
    private static readonly TimeSpan TempoLimiteDeProcessamento = TimeSpan.FromSeconds(15);

    [TestMethod]
    public async Task Aluno_DeveReunirCertificadosDeVariosCursosEBaixarEmPdfOuZip()
    {
        var cliente = await CriarClienteAutenticadoAsync();
        var nomeUnico = $"Ada Lovelace {Guid.CreateVersion7()}";

        var cursoUm = await CriarCursoAsync(cliente, "Curso de Algoritmos");
        await EmitirAsync(cliente, cursoUm, nomeUnico);

        var cursoDois = await CriarCursoAsync(cliente, "Curso de Arquitetura");
        await EmitirAsync(cliente, cursoDois, nomeUnico.ToLowerInvariant());

        // O mesmo nome (mesmo em minúsculas) é um único aluno.
        var respostaAlunos = await cliente.GetAsync($"/alunos?busca={Uri.EscapeDataString(nomeUnico)}");
        Assert.AreEqual(HttpStatusCode.OK, respostaAlunos.StatusCode);

        var alunos = await respostaAlunos.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual(1, alunos.GetArrayLength());

        var alunoId = alunos[0].GetProperty("id").GetGuid();

        // Os certificados dos dois cursos aparecem juntos.
        var respostaCertificados = await cliente.GetAsync($"/alunos/{alunoId}/certificados");
        Assert.AreEqual(HttpStatusCode.OK, respostaCertificados.StatusCode);

        var certificados = await respostaCertificados.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual(2, certificados.GetArrayLength());

        var certificadoId = certificados[0].GetProperty("certificadoId").GetGuid();

        // Um certificado em PDF.
        var respostaPdf = await cliente.GetAsync($"/alunos/{alunoId}/certificados/{certificadoId}/download");
        Assert.AreEqual(HttpStatusCode.OK, respostaPdf.StatusCode);
        Assert.AreEqual("application/pdf", respostaPdf.Content.Headers.ContentType?.MediaType);

        // Todos os certificados em um ZIP.
        var respostaZipTodos = await cliente.PostAsJsonAsync(
            $"/alunos/{alunoId}/certificados/download",
            new { CertificadoIds = Array.Empty<Guid>() });
        Assert.AreEqual(HttpStatusCode.OK, respostaZipTodos.StatusCode);
        Assert.AreEqual("application/zip", respostaZipTodos.Content.Headers.ContentType?.MediaType);

        await using (var conteudoTodos = await respostaZipTodos.Content.ReadAsStreamAsync())
        using (var zipTodos = new ZipArchive(conteudoTodos, ZipArchiveMode.Read))
        {
            Assert.AreEqual(2, zipTodos.Entries.Count);
        }

        // Apenas os certificados escolhidos.
        var respostaZipUm = await cliente.PostAsJsonAsync(
            $"/alunos/{alunoId}/certificados/download",
            new { CertificadoIds = new[] { certificadoId } });
        Assert.AreEqual(HttpStatusCode.OK, respostaZipUm.StatusCode);

        await using var conteudoUm = await respostaZipUm.Content.ReadAsStreamAsync();
        using var zipUm = new ZipArchive(conteudoUm, ZipArchiveMode.Read);
        Assert.AreEqual(1, zipUm.Entries.Count);
    }

    [TestMethod]
    public async Task Aluno_DeveRetornarNaoEncontrado_QuandoAlunoNaoExiste()
    {
        var cliente = await CriarClienteAutenticadoAsync();

        var resposta = await cliente.GetAsync($"/alunos/{Guid.CreateVersion7()}/certificados");

        Assert.AreEqual(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    private static async Task EmitirAsync(HttpClient cliente, Guid cursoId, string nomeAluno)
    {
        var resposta = await cliente.PostAsJsonAsync($"/cursos/{cursoId}/certificados", new
        {
            Alunos = new[] { new { Nome = nomeAluno } }
        });

        Assert.AreEqual(HttpStatusCode.Accepted, resposta.StatusCode);

        var inicio = DateTime.UtcNow;

        while (DateTime.UtcNow - inicio < TempoLimiteDeProcessamento)
        {
            var respostaStatus = await cliente.GetAsync($"/cursos/{cursoId}/status");
            respostaStatus.EnsureSuccessStatusCode();

            var status = await respostaStatus.Content.ReadFromJsonAsync<JsonElement>();
            var statusAtual = status.GetProperty("status").GetString();

            if (statusAtual == "Concluido")
                return;

            if (statusAtual == "Falha")
                Assert.Fail($"A geração dos certificados do curso {cursoId} falhou.");

            await Task.Delay(200);
        }

        Assert.Fail($"O processamento do curso {cursoId} não terminou em {TempoLimiteDeProcessamento.TotalSeconds}s.");
    }
}
