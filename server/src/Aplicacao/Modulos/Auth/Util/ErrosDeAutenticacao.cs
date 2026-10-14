using FluentResults;
using GeradorCertificado.Aplicacao.Compartilhado;
using GeradorCertificado.Dominio.Compartilhado;

namespace GeradorCertificado.Aplicacao.Modulos.Auth.Util;

public static class ErrosDeAutenticacao
{
    public static IEnumerable<Error> Validacao(IEnumerable<ErroValidacao> erros)
    {
        return erros.Select(erro => new Error(erro.Mensagem)
            .WithMetadata(nameof(TipoErro), TipoErro.Validacao)
            .WithMetadata("Campo", erro.Campo));
    }

    public static Error CadastroDuplicado(string mensagem)
    {
        return new Error(mensagem)
            .WithMetadata(nameof(TipoErro), TipoErro.Conflito);
    }

    public static Error CredenciaisInvalidas()
    {
        return new Error("Email ou senha inválidos.")
            .WithMetadata(nameof(TipoErro), TipoErro.NaoAutenticado);
    }
}
