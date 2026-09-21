using FluentResults;
using MediatR;

namespace GeradorCertificado.Aplicacao.Compartilhado
{
    public sealed record AutenticarClienteCommand(
        string Email,
        string Senha
    ) : IRequest<Result<AccessTokenDoUsuarioDto>>;

    public sealed record AutenticarEstabelecimentoCommand(
        string Email,
        string Senha
    ) : IRequest<Result<AccessTokenDoUsuarioDto>>;

    public sealed record AccessTokenDoUsuarioDto(
        Guid UsuarioId,
        string Token,
        DateTime DataExpiracaoEmUtc
    );
}