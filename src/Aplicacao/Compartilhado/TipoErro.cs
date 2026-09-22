using FluentResults;

namespace GeradorCertificado.Aplicacao.Compartilhado;

public enum TipoErro
{
    Validacao,
    NaoEncontrado,
    Conflito,
    NaoAutorizado,
    NaoAutenticado
}

public static class TipoErroExtensions
{
    public static Error ComMetadata(
        this Error erro,
        TipoErro tipo,
        string campo)
    {
        return erro
            .WithMetadata(nameof(TipoErro), tipo)
            .WithMetadata("Campo", campo);
    }
}

