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

// Configuração de opções de serviços
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
    .Configure<IOptions<JwtOptions>>(JwtExtensions.ConfigureJwtBearerValidation);

// Configuração de serviços
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddJwtAuthServices();
builder.Services.AddSerilogServices(builder.Logging);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.ClientErrorMapping[StatusCodes.Status400BadRequest].Link = ProblemDetailsTypes.BadRequest;
        options.ClientErrorMapping[StatusCodes.Status401Unauthorized].Link = ProblemDetailsTypes.Unauthorized;
        options.ClientErrorMapping[StatusCodes.Status403Forbidden].Link = ProblemDetailsTypes.Forbidden;
        options.ClientErrorMapping[StatusCodes.Status404NotFound].Link = ProblemDetailsTypes.NotFound;
        options.ClientErrorMapping[StatusCodes.Status409Conflict].Link = ProblemDetailsTypes.Conflict;
    });

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        string? type = ProblemDetailsTypes.ObterPorStatus(context.ProblemDetails.Status);

        if (type is not null)
            context.ProblemDetails.Type = type;

        if (context.ProblemDetails.Status == StatusCodes.Status401Unauthorized)
        {
            context.ProblemDetails.Title = "Não Autenticado";
            context.ProblemDetails.Detail = "É necessário fornecer credenciais válidas.";
        }
        else if (context.ProblemDetails.Status == StatusCodes.Status403Forbidden)
        {
            context.ProblemDetails.Title = "Acesso Negado";
            context.ProblemDetails.Detail = "O usuário autenticado não tem permissão para acessar este recurso.";
        }

        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();

    var dbContext =
        scope.ServiceProvider.GetRequiredService<GeradorCertificadoDbContext>();

    if (builder.Configuration["Infra:DatabaseProvider"] == "InMemory")
    {
        dbContext.Database.EnsureCreated();
    }
    else
    {
        dbContext.Database.Migrate();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}
else
{
    using var scope = app.Services.CreateScope();

    var dbContext =
        scope.ServiceProvider.GetRequiredService<GeradorCertificadoDbContext>();

    dbContext.Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    await GeradorCertificado.Infra.Compartilhado.Auth.IdentitySeeder
        .SeedRolesAsync(scope.ServiceProvider);
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
