using FluentResults;

namespace GeradorCertificado.Aplicacao.Compartilhado;

public enum TipoErro
{
    Validacao,
    NaoEncontrado,
    Conflito,
<<<<<<< HEAD
    NaoAutorizado
=======
    NaoAutenticado,
    NaoAutorizado,
}

public static class TipoErroExtensions
{
    public static Error ObterMetadados(this TipoErro tipo, string campo, string mensagem)
    {
        return new Error(mensagem)
            .WithMetadata(nameof(TipoErro), tipo)
            .WithMetadata("Campo", campo);
    }
>>>>>>> 96d17c4bf61ad43d88be9c0b8472b7d4c16e937f
}
