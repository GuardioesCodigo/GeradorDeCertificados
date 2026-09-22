using GeradorCertificado.Dominio.Compartilhado.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GeradorCertificado.Infra.Compartilhado.Auth;

public static class IdentitySeeder
{
    /// <summary>
    /// Garante que todos os papéis definidos em <see cref="TipoUsuario"/> existam no banco de dados.
    /// Deve ser chamado uma vez na inicialização da aplicação, em qualquer ambiente, já que o
    /// cadastro de usuários depende desses papéis já existirem (UserManager.AddToRoleAsync falha
    /// silenciosamente com um IdentityError caso o papel não exista).
    /// </summary>
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (TipoUsuario tipo in Enum.GetValues<TipoUsuario>())
        {
            string nomePapel = tipo.ToString();

            if (!await roleManager.RoleExistsAsync(nomePapel))
                await roleManager.CreateAsync(new IdentityRole<Guid>(nomePapel));
        }
    }
}
