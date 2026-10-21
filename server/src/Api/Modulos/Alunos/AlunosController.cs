using GeradorCertificado.Api.Compartilhado.Http;
using GeradorCertificado.Aplicacao.Modulos.Alunos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeradorCertificado.Api.Modulos.Alunos;

[ApiController]
[Route("alunos")]
public sealed class AlunosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<AlunoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlunoResponse>>> Listar(
        [FromQuery] string? busca,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(new ListarAlunosQuery(busca), cancellationToken);

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        var alunos = resultado.Value
            .Select(aluno => new AlunoResponse(aluno.Id, aluno.Nome))
            .ToList();

        return Ok(alunos);
    }

    [HttpGet("{alunoId:guid}")]
    [ProducesResponseType<AlunoResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AlunoResponse>> ObterPorId(
        Guid alunoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(new ObterAlunoPorIdQuery(alunoId), cancellationToken);

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return Ok(new AlunoResponse(resultado.Value.Id, resultado.Value.Nome));
    }

    [HttpGet("{alunoId:guid}/certificados")]
    [ProducesResponseType<List<CertificadoDoAlunoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CertificadoDoAlunoResponse>>> ListarCertificados(
        Guid alunoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new ListarCertificadosDoAlunoQuery(alunoId),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        var certificados = resultado.Value
            .Select(certificado => new CertificadoDoAlunoResponse(
                certificado.CertificadoId,
                certificado.CursoId,
                certificado.CursoNome,
                certificado.CargaHoraria,
                certificado.DataConclusao,
                certificado.DataGeracao
            ))
            .ToList();

        return Ok(certificados);
    }

    [HttpGet("{alunoId:guid}/certificados/{certificadoId:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BaixarCertificado(
        Guid alunoId,
        Guid certificadoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new BaixarCertificadoDoAlunoQuery(alunoId, certificadoId),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        var arquivo = resultado.Value;

        return File(arquivo.Conteudo, arquivo.ContentType, arquivo.NomeArquivo);
    }

    [HttpPost("{alunoId:guid}/certificados/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BaixarCertificadosEmZip(
        Guid alunoId,
        BaixarCertificadosDoAlunoRequest request,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new BaixarCertificadosDoAlunoQuery(alunoId, request.CertificadoIds ?? []),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        var arquivo = resultado.Value;

        return File(arquivo.Conteudo, arquivo.ContentType, arquivo.NomeArquivo);
    }
}
