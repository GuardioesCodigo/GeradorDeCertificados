using GeradorCertificado.Api.Compartilhado.Http;
using GeradorCertificado.Aplicacao.Modulos.Cursos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeradorCertificado.Api.Modulos.Cursos;

[ApiController]
[Route("cursos")]
public sealed class CursosController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CadastrarCursoResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CadastrarCursoResponse>> Cadastrar(
        CadastrarCursoRequest request,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new CadastrarCursoCommand(
                request.Nome,
                request.Descricao ?? string.Empty,
                request.CargaHoraria,
                request.DataConclusao
            ),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { cursoId = resultado.Value },
            new CadastrarCursoResponse(resultado.Value, request.Nome.Trim())
        );
    }

    [HttpGet("{cursoId:guid}")]
    [ProducesResponseType<CursoResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursoResponse>> ObterPorId(
        Guid cursoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new ObterCursoPorIdQuery(cursoId),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        var curso = resultado.Value;

        return Ok(new CursoResponse(
            curso.Id,
            curso.Nome,
            curso.Descricao,
            curso.CargaHoraria,
            curso.DataConclusao
        ));
    }
}
