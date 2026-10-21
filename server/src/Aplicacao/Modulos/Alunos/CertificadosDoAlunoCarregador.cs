using GeradorCertificado.Dominio.Modulos.Cursos;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;

namespace GeradorCertificado.Aplicacao.Modulos.Alunos;

internal sealed record CertificadoDoAluno(Certificado Certificado, Curso Curso);

/// <summary>
/// Reúne, para um aluno, os certificados já gerados em todos os cursos (do mais recente
/// para o mais antigo), junto com o curso de cada um.
/// </summary>
internal static class CertificadosDoAlunoCarregador
{
    public static async Task<List<CertificadoDoAluno>> CarregarAsync(
        Guid alunoId,
        IRepositorioCertificado repositorioCertificado,
        IRepositorioSolicitacaoCertificado repositorioSolicitacao,
        IRepositorioCurso repositorioCurso,
        CancellationToken cancellationToken
    )
    {
        var certificados = await repositorioCertificado.SelecionarGeradosPorAlunoAsync(
            alunoId,
            cancellationToken
        );

        Dictionary<Guid, Curso?> cursoPorSolicitacao = [];
        Dictionary<Guid, Curso?> cursoPorId = [];
        List<CertificadoDoAluno> resultado = [];

        foreach (Certificado certificado in certificados)
        {
            if (!cursoPorSolicitacao.TryGetValue(certificado.SolicitacaoCertificadoId, out Curso? curso))
            {
                var solicitacao = await repositorioSolicitacao.SelecionarPorIdAsync(
                    certificado.SolicitacaoCertificadoId,
                    cancellationToken
                );

                curso = null;

                if (solicitacao is not null &&
                    !cursoPorId.TryGetValue(solicitacao.CursoId, out curso))
                {
                    curso = await repositorioCurso.SelecionarPorIdAsync(
                        solicitacao.CursoId,
                        cancellationToken
                    );

                    cursoPorId[solicitacao.CursoId] = curso;
                }

                cursoPorSolicitacao[certificado.SolicitacaoCertificadoId] = curso;
            }

            if (curso is not null)
                resultado.Add(new CertificadoDoAluno(certificado, curso));
        }

        return resultado;
    }
}
