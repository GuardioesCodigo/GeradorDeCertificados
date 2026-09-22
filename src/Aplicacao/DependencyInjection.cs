using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Mensageria;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeradorCertificado.Aplicacao;

public static class DependencyInjection
{
public static void AddApplicationServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddMassTransit(config =>
    {
        config.SetKebabCaseEndpointNameFormatter();
        config.AddConsumer<GerarCertificadosConsumer>();

        var messageBrokerProvider =
            configuration["Infra:MessageBrokerProvider"]?.Trim();

        if (string.Equals(
            messageBrokerProvider,
            "InMemory",
            StringComparison.OrdinalIgnoreCase))
        {
            config.UsingInMemory((context, inMemory) =>
            {
                inMemory.ConfigureEndpoints(context);
            });

            return;
        }

        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq")
            ?? throw new InvalidOperationException(
                "A ConnectionString \"RabbitMq\" não foi configurada");

        config.UsingRabbitMq((context, rabbitMq) =>
        {
            rabbitMq.Host(new Uri(rabbitMqConnectionString));

            rabbitMq.ReceiveEndpoint("certificados-gerados", endpoint =>
            {
                endpoint.PrefetchCount = 4;
                endpoint.ConcurrentMessageLimit = 2;
                endpoint.UseMessageRetry(DefaultMessageRetryIntervals);
                endpoint.ConfigureConsumer<GerarCertificadosConsumer>(context);
            });
        });
    });

    services.Configure<MassTransitHostOptions>(options =>
    {
        options.WaitUntilStarted = true;
        options.StartTimeout = TimeSpan.FromSeconds(30);
    });
}

    private static void DefaultMessageRetryIntervals(IRetryConfigurator retry) => retry.Intervals(
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(15)
    );
}
