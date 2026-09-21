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
        IConfiguration configuration
    )
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            // Configura a injeção dos Consumers
            config.AddConsumer<GerarCertificadosConsumer>();

            if (configuration["Infra:MessageBrokerProvider"] == "InMemory")
            {
                // Usado nos testes de integração, para não depender de um RabbitMQ real.
                config.UsingInMemory((context, inMemory) =>
                {
                    inMemory.ConfigureEndpoints(context);
                });
            }
            else
            {
                var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq")
                    ?? throw new InvalidOperationException("A ConnectionString \"RabbitMq\" não foi configurada");

                config.UsingRabbitMq((context, rabbitMq) =>
                {
                    rabbitMq.Host(new Uri(rabbitMqConnectionString));

                    rabbitMq.ReceiveEndpoint("certificados-gerados", endpoint =>
                    {
                        endpoint.PrefetchCount = 4; // Quantas mensagens o RabbitMQ deve carregar adiantado
                        endpoint.ConcurrentMessageLimit = 2; // Quantos consumers serão instanciados em paralelo
                        endpoint.UseMessageRetry(DefaultMessageRetryIntervals); // Quantas re-tentativas serão feitas e o intervalo entre elas

                        endpoint.ConfigureConsumer<GerarCertificadosConsumer>(context);
                    });
                });
            }
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
