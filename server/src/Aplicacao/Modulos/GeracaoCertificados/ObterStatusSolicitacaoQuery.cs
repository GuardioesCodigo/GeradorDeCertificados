using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.DTOs;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Util;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados;

public sealed record ObterStatusSolicitacaoQuery(
    Guid CursoId
) : IRequest<Result<StatusSolicitacaoDto>>;

public sealed class ObterStatusSolicitacaoQueryHandler(
    IRepositorioSolicitacaoCertificado repositorioSolicitacao
) : IRequestHandler<ObterStatusSolicitacaoQuery, Result<StatusSolicitacaoDto>>
{
    public async Task<Result<StatusSolicitacaoDto>> Handle(
        ObterStatusSolicitacaoQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var solicitacao = await repositorioSolicitacao.SelecionarMaisRecentePorCursoAsync(
            query.CursoId,
            cancellationToken
        );

        if (solicitacao is null)
            return Result.Fail(ErrosDeSolicitacaoCertificado.NenhumaSolicitacaoEncontrada(query.CursoId));

        return Result.Ok(new StatusSolicitacaoDto(
            solicitacao.Id,
            solicitacao.CursoId,
            solicitacao.StatusSolicitacao.ToString(),
            solicitacao.DataSolicitacao,
            solicitacao.Certificados.Count,
            solicitacao.Certificados.Count(c => c.Status == StatusCertificado.Gerado),
            solicitacao.Certificados.Count(c => c.Status == StatusCertificado.Falha)
        ));
    }
}
