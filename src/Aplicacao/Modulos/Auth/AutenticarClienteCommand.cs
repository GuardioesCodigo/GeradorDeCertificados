using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.Auth.Util;
using GeradorCertificado.Dominio.Compartilhado.Auth;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.Auth;

public sealed record AutenticarClienteCommand(
    string Email,
    string Senha
) : IRequest<Result<AccessTokenDoUsuarioDto>>;

public sealed record AccessTokenDoUsuarioDto(
    Guid UsuarioId,
    string Token,
    DateTime DataExpiracaoEmUtc
);

public sealed class AutenticarClienteCommandHandler(
    IGerenciadorDeIdentidade gerenciadorDeIdentidade,
    IEmissorDeTokens emissorDeTokens
) : IRequestHandler<AutenticarClienteCommand, Result<AccessTokenDoUsuarioDto>>
{
    public async Task<Result<AccessTokenDoUsuarioDto>> Handle(
        AutenticarClienteCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Senha))
            return Result.Fail(ErrosDeAutenticacao.CredenciaisInvalidas());

        var usuario = await gerenciadorDeIdentidade.ChecarValidadeDeSenhaAsync(
            command.Email.Trim(),
            command.Senha,
            TipoUsuario.Cliente
        );

        if (usuario is null)
            return Result.Fail(ErrosDeAutenticacao.CredenciaisInvalidas());

        AccessToken token = emissorDeTokens.CriarToken(usuario.Id, usuario.Email, TipoUsuario.Cliente);

        return Result.Ok(new AccessTokenDoUsuarioDto(usuario.Id, token.Token, token.DataExpiracaoEmUtc));
    }
}
