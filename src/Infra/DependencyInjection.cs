using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Servicos;
using GeradorCertificado.Dominio.Modulos.Cursos;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using GeradorCertificado.Infra.Compartilhado.Orm;
using GeradorCertificado.Infra.Modulos.Cursos;
using GeradorCertificado.Infra.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Infra.Modulos.GeracaoCertificados.Servicos;
using GeradorCertificado.Infra.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeradorCertificado.Infra;

public static class DependencyInjection
{
public static void AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<IRepositorioCurso, RepositorioCursoEmOrm>();
        services.AddScoped<IRepositorioCertificado, RepositorioCertificadoEmOrm>();
        services.AddScoped<IRepositorioSolicitacaoCertificado, RepositorioSolicitacaoCertificadoEmOrm>();

        services.AddSingleton<IGeradorDeCertificadoPdf, GeradorDeCertificadoPdfComQuestPdf>();
        services.AddSingleton<IArmazenamentoDeArquivos, ArmazenamentoDeArquivosEmDisco>();

        // services.AddDataProtection();
        // services.AddIdentityCore<IdentityUser<Guid>>(options =>
        // {
        //     options.User.RequireUniqueEmail = true;
        //     options.SignIn.RequireConfirmedEmail = false;
        //     options.Password.RequiredLength = 8;
        //     options.Password.RequireDigit = true;
        //     options.Password.RequireNonAlphanumeric = true;
        //     options.Password.RequireUppercase = false;
        //     options.Password.RequireLowercase = false;
        //     options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        //     options.Lockout.MaxFailedAccessAttempts = 5;
        //     options.Lockout.AllowedForNewUsers = true;
        // })
        // .AddRoles<IdentityRole<Guid>>()
        // // .AddEntityFrameworkStores<GeradorCertificadoDbContext>()
        // .AddSignInManager()
        // .AddDefaultTokenProviders();

        services.AddDbContext<GeradorCertificadoDbContext>(options =>
        {
            if (configuration["Infra:DatabaseProvider"] == "InMemory")
            {
                options.UseInMemoryDatabase("GeradorCertificado");
            }
            else
            {
                string? connectionString = configuration.GetConnectionString("PostgresEF");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        $"A connection string \"PostgresEF\" não foi encontrada."
                    );
                }

                options.UseNpgsql(connectionString, opt =>
                {
                    opt.EnableRetryOnFailure(3);
                });
            }
        });
    }
}
