using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.Alunos.DTOs;
using GeradorCertificado.Aplicacao.Modulos.Alunos.Util;
using GeradorCertificado.Dominio.Modulos.Alunos;
using GeradorCertificado.Dominio.Modulos.Cursos;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.Alunos;

public sealed record ListarCertificadosDoAlunoQuery(
    Guid AlunoId
) : IRequest<Result<List<CertificadoDoAlunoDto>>>;

public sealed class ListarCertificadosDoAlunoQueryHandler(
    IRepositorioAluno repositorioAluno,
    IRepositorioCertificado repositorioCertificado,
    IRepositorioSolicitacaoCertificado repositorioSolicitacao,
    IRepositorioCurso repositorioCurso
) : IRequestHandler<ListarCertificadosDoAlunoQuery, Result<List<CertificadoDoAlunoDto>>>
{
    public async Task<Result<List<CertificadoDoAlunoDto>>> Handle(
        ListarCertificadosDoAlunoQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var aluno = await repositorioAluno.SelecionarPorIdAsync(query.AlunoId, cancellationToken);

        if (aluno is null)
            return Result.Fail<List<CertificadoDoAlunoDto>>(ErrosDeAluno.AlunoNaoEncontrado(query.AlunoId));

        var itens = await CertificadosDoAlunoCarregador.CarregarAsync(
            query.AlunoId,
            repositorioCertificado,
            repositorioSolicitacao,
            repositorioCurso,
            cancellationToken
        );

        var dtos = itens
            .Select(item => new CertificadoDoAlunoDto(
                item.Certificado.Id,
                item.Curso.Id,
                item.Curso.Nome,
                item.Curso.CargaHoraria,
                item.Curso.DataConclusao,
                item.Certificado.DataGeracao
            ))
            .ToList();

        return Result.Ok(dtos);
    }
}
