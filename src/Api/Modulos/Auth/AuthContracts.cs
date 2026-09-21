namespace GeradorCertificado.Api.Modulos.Auth;

public sealed record CadastrarUsuarioRequest(
    string Email,
    string Senha
);

public sealed record CadastrarUsuarioResponse(
    Guid Id,
    string Email
);

public sealed record AutenticarUsuarioRequest(
    string Email,
    string Senha
);

public sealed record AutenticarUsuarioResponse(
    string Token,
    DateTime DataExpiracaoEmUtc
);
