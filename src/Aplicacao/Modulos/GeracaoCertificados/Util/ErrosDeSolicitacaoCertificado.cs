using FluentResults;
using GeradorCertificado.Aplicacao.Compartilhado;
using GeradorCertificado.Dominio.Compartilhado;

namespace GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Util;

public static class ErrosDeSolicitacaoCertificado
{
    public static Error CursoNaoEncontrado(Guid cursoId)
    {
        return new Error("O curso com este ID não foi encontrado.")
            .WithMetadata(nameof(TipoErro), TipoErro.NaoEncontrado)
            .WithMetadata("CursoId", cursoId);
    }

    public static Error ProcessamentoEmAndamento(Guid cursoId)
    {
        return new Error("Já existe uma solicitação de geração de certificados em andamento para este curso.")
            .WithMetadata(nameof(TipoErro), TipoErro.Conflito)
            .WithMetadata("CursoId", cursoId);
    }

    public static Error NenhumaSolicitacaoEncontrada(Guid cursoId)
    {
        return new Error("Nenhuma solicitação de geração de certificados foi encontrada para este curso.")
            .WithMetadata(nameof(TipoErro), TipoErro.NaoEncontrado)
            .WithMetadata("CursoId", cursoId);
    }

    public static Error ZipAindaNaoDisponivel(Guid cursoId)
    {
        return new Error("O arquivo ZIP com os certificados ainda não está disponível para download.")
            .WithMetadata(nameof(TipoErro), TipoErro.NaoEncontrado)
            .WithMetadata("CursoId", cursoId);
    }

    public static IEnumerable<Error> Validacao(IEnumerable<ErroValidacao> erros)
    {
        return erros.Select(erro => new Error(erro.Mensagem)
            .WithMetadata(nameof(TipoErro), TipoErro.Validacao)
            .WithMetadata("Campo", erro.Campo));
    }
}
