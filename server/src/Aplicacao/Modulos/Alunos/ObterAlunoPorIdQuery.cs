using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.Alunos.DTOs;
using GeradorCertificado.Aplicacao.Modulos.Alunos.Util;
using GeradorCertificado.Dominio.Modulos.Alunos;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.Alunos;

public sealed record ObterAlunoPorIdQuery(
    Guid AlunoId
) : IRequest<Result<AlunoDto>>;

public sealed class ObterAlunoPorIdQueryHandler(
    IRepositorioAluno repositorioAluno
) : IRequestHandler<ObterAlunoPorIdQuery, Result<AlunoDto>>
{
    public async Task<Result<AlunoDto>> Handle(
        ObterAlunoPorIdQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var aluno = await repositorioAluno.SelecionarPorIdAsync(query.AlunoId, cancellationToken);

        if (aluno is null)
            return Result.Fail<AlunoDto>(ErrosDeAluno.AlunoNaoEncontrado(query.AlunoId));

        return Result.Ok(new AlunoDto(aluno.Id, aluno.Nome));
    }
}
