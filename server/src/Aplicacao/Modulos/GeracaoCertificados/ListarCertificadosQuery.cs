using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.DTOs;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Util;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados;

public sealed record ListarCertificadosQuery(
    Guid CursoId
) : IRequest<Result<List<CertificadoDto>>>;

public sealed class ListarCertificadosQueryHandler(
    IRepositorioSolicitacaoCertificado repositorioSolicitacao
) : IRequestHandler<ListarCertificadosQuery, Result<List<CertificadoDto>>>
{
    public async Task<Result<List<CertificadoDto>>> Handle(
        ListarCertificadosQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var solicitacao = await repositorioSolicitacao.SelecionarMaisRecentePorCursoAsync(
            query.CursoId,
            cancellationToken
        );

        if (solicitacao is null)
            return Result.Fail(ErrosDeSolicitacaoCertificado.NenhumaSolicitacaoEncontrada(query.CursoId));

        var certificados = solicitacao.Certificados
            .Select(certificado => new CertificadoDto(
                certificado.Id,
                certificado.NomeAluno,
                certificado.Status.ToString(),
                certificado.CaminhoArquivo,
                certificado.DataGeracao
            ))
            .ToList();

        return Result.Ok(certificados);
    }
}
