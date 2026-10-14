using GeradorCertificado.Aplicacao.Modulos.Cursos.DTOs;
using GeradorCertificado.Aplicacao.Modulos.Cursos.Util;
using GeradorCertificado.Dominio.Modulos.Cursos;
using FluentResults;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.Cursos;

public sealed record ObterCursoPorIdQuery(
    Guid CursoId
) : IRequest<Result<CursoDto>>;

public sealed class ObterCursoPorIdQueryHandler(
    IRepositorioCurso repositorioCurso
) : IRequestHandler<ObterCursoPorIdQuery, Result<CursoDto>>
{
    public async Task<Result<CursoDto>> Handle(
        ObterCursoPorIdQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var curso = await repositorioCurso.SelecionarPorIdAsync(
            query.CursoId,
            cancellationToken
        );

        if (curso is null)
            return Result.Fail(
                ErrosDeCurso.NaoEncontrado(query.CursoId)
            );

        return Result.Ok(new CursoDto(
            curso.Id,
            curso.Nome,
            curso.Descricao,
            curso.CargaHoraria,
            curso.DataConclusao
        ));
    }
}