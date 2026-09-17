using Microsoft.Extensions.DependencyInjection;

namespace GeradorCertificado.Aplicacao;

public static class DepedencyInjection
{
    public static void AddApplicationServices(
        this IServiceCollection services
    )
    {
        using var serviceProvider = services.BuildServiceProvider();
    }
}
