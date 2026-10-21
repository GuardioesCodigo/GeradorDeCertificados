using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.Alunos.DTOs;
using GeradorCertificado.Dominio.Modulos.Alunos;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.Alunos;

public sealed record ListarAlunosQuery(
    string? Busca
) : IRequest<Result<List<AlunoDto>>>;

public sealed class ListarAlunosQueryHandler(
    IRepositorioAluno repositorioAluno
) : IRequestHandler<ListarAlunosQuery, Result<List<AlunoDto>>>
{
    public async Task<Result<List<AlunoDto>>> Handle(
        ListarAlunosQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var alunos = await repositorioAluno.BuscarPorNomeAsync(query.Busca, cancellationToken);

        var dtos = alunos
            .Select(aluno => new AlunoDto(aluno.Id, aluno.Nome))
            .ToList();

        return Result.Ok(dtos);
    }
}
