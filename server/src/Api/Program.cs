using System.Diagnostics;
using System.Text.Json.Serialization;
using GeradorCertificado.Api.Compartilhado.Auth;
using GeradorCertificado.Api.Compartilhado.Http;
using GeradorCertificado.Api.Compartilhado.Logging;
using GeradorCertificado.Infra.Compartilhado.Orm;
using GeradorCertificado.Infra;
using GeradorCertificado.Aplicacao;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// DIAGNÓSTICO DE STARTUP
// ============================================================

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    Console.Error.WriteLine(
        $"[UNHANDLED EXCEPTION] {e.ExceptionObject}");
};

TaskScheduler.UnobservedTaskException += (_, e) =>
{
    Console.Error.WriteLine(
        $"[UNOBSERVED TASK EXCEPTION] {e.Exception}");

    e.SetObserved();
};

Console.WriteLine("==================================================");
Console.WriteLine("INICIANDO GERADOR CERTIFICADO API");
Console.WriteLine("==================================================");

Console.WriteLine(
    $"Environment: {builder.Environment.EnvironmentName}");

Console.WriteLine(
    $"DatabaseProvider: {builder.Configuration["Infra:DatabaseProvider"] ?? "(não configurado)"}");

Console.WriteLine(
    $"AzureSQL configurado: {!string.IsNullOrWhiteSpace(
        builder.Configuration.GetConnectionString("AzureSQL"))}");

Console.WriteLine(
    $"JWT Key configurada: {!string.IsNullOrWhiteSpace(
        builder.Configuration["Jwt:Key"])}");

Console.WriteLine(
    $"JWT Issuer configurado: {!string.IsNullOrWhiteSpace(
        builder.Configuration["Jwt:Issuer"])}");

Console.WriteLine(
    $"JWT Audience configurado: {!string.IsNullOrWhiteSpace(
        builder.Configuration["Jwt:Audience"])}");

Console.WriteLine(
    $"NewRelic Enabled: {builder.Configuration["NewRelic:Enabled"] ?? "(não configurado)"}");

Console.WriteLine(
    $"NewRelic License configurada: {!string.IsNullOrWhiteSpace(
        builder.Configuration["NewRelic:LicenseKey"])}");

// ============================================================
// CONFIGURAÇÃO DE OPÇÕES
// ============================================================

builder.Services
    .AddOptions<NewRelicOptions>()
    .BindConfiguration(NewRelicOptions.SectionName);

builder.Services
    .AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Audience))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key))
    .ValidateOnStart();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>(
        JwtExtensions.ConfigureJwtBearerValidation);

// ============================================================
// CONFIGURAÇÃO DOS SERVIÇOS
// ============================================================

try
{
    Console.WriteLine("Configurando Infrastructure...");

    builder.Services.AddInfrastructureServices(
        builder.Configuration);

    Console.WriteLine("Infrastructure configurada.");

    Console.WriteLine("Configurando Application...");

    builder.Services.AddApplicationServices(
        builder.Configuration);

    Console.WriteLine("Application configurada.");

    Console.WriteLine("Configurando JWT...");

    builder.Services.AddJwtAuthServices();

    Console.WriteLine("JWT configurado.");

    Console.WriteLine("Configurando Serilog/New Relic...");

    builder.Services.AddSerilogServices(
        builder.Logging);

    Console.WriteLine("Serilog/New Relic configurado.");
}
catch (Exception ex)
{
    Console.Error.WriteLine(
        "==================================================");

    Console.Error.WriteLine(
        "ERRO DURANTE A CONFIGURAÇÃO DOS SERVIÇOS");

    Console.Error.WriteLine(
        ex.ToString());

    Console.Error.WriteLine(
        "==================================================");

    throw;
}

// ============================================================
// CONTROLLERS
// ============================================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.ClientErrorMapping[
            StatusCodes.Status400BadRequest
        ].Link = ProblemDetailsTypes.BadRequest;

        options.ClientErrorMapping[
            StatusCodes.Status401Unauthorized
        ].Link = ProblemDetailsTypes.Unauthorized;

        options.ClientErrorMapping[
            StatusCodes.Status403Forbidden
        ].Link = ProblemDetailsTypes.Forbidden;

        options.ClientErrorMapping[
            StatusCodes.Status404NotFound
        ].Link = ProblemDetailsTypes.NotFound;

        options.ClientErrorMapping[
            StatusCodes.Status409Conflict
        ].Link = ProblemDetailsTypes.Conflict;
    });

// ============================================================
// PROBLEM DETAILS
// ============================================================

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        string? type =
            ProblemDetailsTypes.ObterPorStatus(
                context.ProblemDetails.Status);

        if (type is not null)
        {
            context.ProblemDetails.Type = type;
        }

        if (context.ProblemDetails.Status ==
            StatusCodes.Status401Unauthorized)
        {
            context.ProblemDetails.Title =
                "Não Autenticado";

            context.ProblemDetails.Detail =
                "É necessário fornecer credenciais válidas.";
        }
        else if (context.ProblemDetails.Status ==
                 StatusCodes.Status403Forbidden)
        {
            context.ProblemDetails.Title =
                "Acesso Negado";

            context.ProblemDetails.Detail =
                "O usuário autenticado não tem permissão para acessar este recurso.";
        }

        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ??
            context.HttpContext.TraceIdentifier;
    };
});

// ============================================================
// OPENAPI / SWAGGER
// ============================================================

builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Informe o token JWT no formato: Bearer {token}"
        });

    options.AddSecurityRequirement(
        document =>
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        document,
                        null)
                ] = []
            });
});

// ============================================================
// BUILD DA APLICAÇÃO
// ============================================================

WebApplication app;

try
{
    Console.WriteLine("Executando builder.Build()...");

    app = builder.Build();

    Console.WriteLine("builder.Build() concluído.");
}
catch (Exception ex)
{
    Console.Error.WriteLine(
        "==================================================");

    Console.Error.WriteLine(
        "ERRO DURANTE builder.Build()");

    Console.Error.WriteLine(
        ex.ToString());

    Console.Error.WriteLine(
        "==================================================");

    throw;
}

// ============================================================
// SWAGGER
// ============================================================

if (app.Environment.IsDevelopment() ||
    app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// ============================================================
// BANCO DE DADOS
// ============================================================

if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Testing"))
{
    Console.WriteLine(
        "Ambiente Development/Testing detectado.");

    try
    {
        using var scope =
            app.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<GeradorCertificadoDbContext>();

        Console.WriteLine(
            "DbContext obtido com sucesso.");

        if (builder.Configuration[
                "Infra:DatabaseProvider"] == "InMemory")
        {
            Console.WriteLine(
                "Usando banco InMemory.");

            dbContext.Database.EnsureCreated();

            Console.WriteLine(
                "Banco InMemory criado.");
        }
        else
        {
            Console.WriteLine(
                "Executando Database.Migrate()...");

            dbContext.Database.Migrate();

            Console.WriteLine(
                "Database.Migrate() concluído.");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(
            "==================================================");

        Console.Error.WriteLine(
            "ERRO DURANTE A CONFIGURAÇÃO DO BANCO");

        Console.Error.WriteLine(
            ex.ToString());

        Console.Error.WriteLine(
            "==================================================");

        throw;
    }
}

// ============================================================
// IDENTITY SEEDER
// ============================================================

try
{
    Console.WriteLine(
        "==================================================");

    Console.WriteLine(
        "INICIANDO IDENTITY SEEDER");

    Console.WriteLine(
        "==================================================");

    using var scope =
        app.Services.CreateScope();

    await GeradorCertificado
        .Infra
        .Compartilhado
        .Auth
        .IdentitySeeder
        .SeedRolesAsync(
            scope.ServiceProvider);

    Console.WriteLine(
        "==================================================");

    Console.WriteLine(
        "IDENTITY SEEDER CONCLUÍDO");

    Console.WriteLine(
        "==================================================");
}
catch (Exception ex)
{
    Console.Error.WriteLine(
        "==================================================");

    Console.Error.WriteLine(
        "ERRO NO IDENTITY SEEDER");

    Console.Error.WriteLine(
        ex.ToString());

    Console.Error.WriteLine(
        "==================================================");

    throw;
}

// ============================================================
// PIPELINE HTTP
// ============================================================

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// ============================================================
// START
// ============================================================

Console.WriteLine(
    "==================================================");

Console.WriteLine(
    "GERADOR CERTIFICADO API INICIADO COM SUCESSO");

Console.WriteLine(
    "==================================================");

app.Run();

public partial class Program
{
}
