using GeradorCertificado.Aplicacao.Modulos.Cursos.Util;
using GeradorCertificado.Dominio.Modulos.Cursos;
using FluentResults;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.Cursos;

public sealed record CadastrarCursoCommand(
    string Nome,
    string Descricao,
    int CargaHoraria,
    DateTime DataConclusao
) : IRequest<Result<Guid>>;

public sealed class CadastrarCursoCommandHandler(
    IRepositorioCurso repositorioCurso
) : IRequestHandler<CadastrarCursoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CadastrarCursoCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var curso = new Curso(
            Guid.CreateVersion7(),
            command.Nome,
            command.Descricao,
            command.CargaHoraria,
            command.DataConclusao
        );

        var erros = curso.Validar();

        if (erros.Count > 0)
            return Result.Fail(ErrosDeCurso.Validacao(erros));

        await repositorioCurso.CadastrarAsync(
            curso,
            cancellationToken
        );

        return Result.Ok(curso.Id);
    }
}