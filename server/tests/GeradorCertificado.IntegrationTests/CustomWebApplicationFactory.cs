using GeradorCertificado.Infra.Compartilhado.Orm;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GeradorCertificado.IntegrationTests;

/// <summary>
/// Cada teste recebe sua própria instância desta factory,
/// com banco InMemory isolado por instância.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string nomeDoBanco =
        $"GeradorCertificado-Testes-{Guid.NewGuid()}";

    public string CaminhoArmazenamento { get; } =
        Path.Combine(
            Path.GetTempPath(),
            "GeradorCertificadoTests",
            Guid.NewGuid().ToString());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting(
            "Infra:DatabaseProvider",
            "InMemory");

        builder.UseSetting(
            "Infra:MessageBrokerProvider",
            "InMemory");

        builder.UseSetting(
            "Armazenamento:CaminhoBase",
            CaminhoArmazenamento);

        builder.UseSetting(
            "NewRelic:Enabled",
            "false");

        builder.UseSetting(
            "Jwt:Key",
            "chave-de-teste-para-assinatura-jwt-usada-somente-nos-testes-automatizados");

        builder.ConfigureServices(services =>
        {
            var descritorExistente = services.SingleOrDefault(
                servico =>
                    servico.ServiceType ==
                    typeof(DbContextOptions<GeradorCertificadoDbContext>));

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
                Directory.Delete(
                    CaminhoArmazenamento,
                    recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}