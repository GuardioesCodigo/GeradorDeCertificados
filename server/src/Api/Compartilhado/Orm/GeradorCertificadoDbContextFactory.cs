using GeradorCertificado.Infra.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GeradorCertificado.Api.Compartilhado.Orm;

public sealed class GeradorCertificadoDbContextFactory
    : IDesignTimeDbContextFactory<GeradorCertificadoDbContext>
{
    public GeradorCertificadoDbContext CreateDbContext(string[] args)
    {
        string? connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__AzureSQL");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A variável de ambiente ConnectionStrings__AzureSQL não foi configurada."
            );
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<GeradorCertificadoDbContext>();

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(
                    typeof(GeradorCertificadoDbContext).Assembly.FullName
                );

                sqlOptions.EnableRetryOnFailure(3);
            });

        return new GeradorCertificadoDbContext(optionsBuilder.Options);
    }
}