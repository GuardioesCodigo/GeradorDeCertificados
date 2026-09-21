using GeradorCertificado.Infra.Compartilhado.Orm;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeradorCertificado.IntegrationTests;

/// <summary>
/// Cada teste recebe sua própria instância desta factory (veja BaseIntegrationTest),
/// com um banco InMemory isolado por instância — testes não compartilham dados entre si.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string nomeDoBanco = $"GeradorCertificado-Testes-{Guid.NewGuid()}";
    public string CaminhoArmazenamento { get; } =
        Path.Combine(Path.GetTempPath(), "GeradorCertificadoTests", Guid.NewGuid().ToString());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Chave só para os testes — nunca usada fora deste processo.
                ["Jwt:Key"] = "chave-de-teste-para-assinatura-jwt-usada-somente-nos-testes-automatizados",
                ["Infra:DatabaseProvider"] = "InMemory",
                ["Infra:MessageBrokerProvider"] = "InMemory",
                ["Armazenamento:CaminhoBase"] = CaminhoArmazenamento,
                // Evita que o host quebre no boot tentando validar uma licença do New Relic.
                ["NewRelic:Enabled"] = "false",
            });
        });

        builder.ConfigureServices(services =>
        {
            var descritorExistente = services.SingleOrDefault(
                servico => servico.ServiceType == typeof(DbContextOptions<GeradorCertificadoDbContext>));

            if (descritorExistente is not null)
                services.Remove(descritorExistente);

            services.AddDbContext<GeradorCertificadoDbContext>(options =>
            {
                options.UseInMemoryDatabase(nomeDoBanco);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(CaminhoArmazenamento))
        {
            try
            {
                Directory.Delete(CaminhoArmazenamento, recursive: true);
            }
            catch (IOException)
            {
                // Best-effort: não falha o teste por não conseguir limpar a pasta temporária.
            }
        }
    }
}
