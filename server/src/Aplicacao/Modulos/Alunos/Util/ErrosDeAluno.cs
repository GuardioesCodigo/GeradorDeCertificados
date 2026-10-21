using FluentResults;
using GeradorCertificado.Aplicacao.Compartilhado;

namespace GeradorCertificado.Aplicacao.Modulos.Alunos.Util;

public static class ErrosDeAluno
{
    public static Error AlunoNaoEncontrado(Guid alunoId)
    {
        return new Error("O aluno com este ID não foi encontrado.")
            .WithMetadata(nameof(TipoErro), TipoErro.NaoEncontrado)
            .WithMetadata("AlunoId", alunoId);
    }

    public static Error CertificadoNaoEncontrado(Guid alunoId, Guid certificadoId)
    {
        return new Error("O certificado informado não foi encontrado para este aluno.")
            .WithMetadata(nameof(TipoErro), TipoErro.NaoEncontrado)
            .WithMetadata("AlunoId", alunoId)
            .WithMetadata("CertificadoId", certificadoId);
    }

    public static Error NenhumCertificadoDisponivel(Guid alunoId)
    {
        return new Error("Este aluno ainda não possui certificados gerados para download.")
            .WithMetadata(nameof(TipoErro), TipoErro.NaoEncontrado)
            .WithMetadata("AlunoId", alunoId);
    }

    public static Error ArquivoIndisponivel(Guid certificadoId)
    {
        return new Error("O arquivo deste certificado não está mais disponível. Emita o certificado novamente.")
            .WithMetadata(nameof(TipoErro), TipoErro.NaoEncontrado)
            .WithMetadata("CertificadoId", certificadoId);
    }
}
