using GeradorCertificado.Api.Compartilhado.Http;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GeradorCertificado.Api.Modulos.GeracaoCertificados;

[ApiController]
[Route("cursos/{cursoId:guid}")]
public sealed class CertificadosController(IMediator mediator) : ControllerBase
{
    [HttpPost("certificados")]
    [ProducesResponseType<SolicitarGeracaoCertificadosResponse>(StatusCodes.Status202Accepted)]
    public async Task<ActionResult<SolicitarGeracaoCertificadosResponse>> SolicitarGeracao(
        Guid cursoId,
        SolicitarGeracaoCertificadosRequest request,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new SolicitarGeracaoCertificadosCommand(
                cursoId,
                request.Alunos
                    .Select(aluno => new AlunoParaCertificadoCommand(aluno.Nome))
                    .ToList()
            ),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        return AcceptedAtAction(
            nameof(ObterStatus),
            new { cursoId },
            new SolicitarGeracaoCertificadosResponse(resultado.Value, cursoId)
        );
    }

    [HttpGet("status")]
    [ProducesResponseType<StatusSolicitacaoResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StatusSolicitacaoResponse>> ObterStatus(
        Guid cursoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new ObterStatusSolicitacaoQuery(cursoId),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        var status = resultado.Value;

        return Ok(new StatusSolicitacaoResponse(
            status.SolicitacaoId,
            status.CursoId,
            status.Status,
            status.DataSolicitacao,
            status.TotalCertificados,
            status.CertificadosGerados,
            status.CertificadosComFalha
        ));
    }

    [HttpGet("certificados")]
    [ProducesResponseType<List<CertificadoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CertificadoResponse>>> ListarCertificados(
        Guid cursoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new ListarCertificadosQuery(cursoId),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        var certificados = resultado.Value
            .Select(certificado => new CertificadoResponse(
                certificado.Id,
                certificado.NomeAluno,
                certificado.Status,
                certificado.CaminhoArquivo,
                certificado.DataGeracao
            ))
            .ToList();

        return Ok(certificados);
    }

    [HttpGet("certificados/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BaixarZip(
        Guid cursoId,
        CancellationToken cancellationToken
    )
    {
        var resultado = await mediator.Send(
            new ObterZipCertificadosQuery(cursoId),
            cancellationToken
        );

        if (resultado.IsFailed)
            return this.ProblemDetails(resultado);

        var arquivo = resultado.Value;

        return File(arquivo.Conteudo, arquivo.ContentType, arquivo.NomeArquivo);
    }
}
