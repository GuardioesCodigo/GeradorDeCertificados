using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.DTOs;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Servicos;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Util;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados;

public sealed record ObterZipCertificadosQuery(
    Guid CursoId
) : IRequest<Result<ArquivoDto>>;

public sealed class ObterZipCertificadosQueryHandler(
    IRepositorioSolicitacaoCertificado repositorioSolicitacao,
    IArmazenamentoDeArquivos armazenamento
) : IRequestHandler<ObterZipCertificadosQuery, Result<ArquivoDto>>
{
    public async Task<Result<ArquivoDto>> Handle(
        ObterZipCertificadosQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var solicitacao = await repositorioSolicitacao.SelecionarMaisRecentePorCursoAsync(
            query.CursoId,
            cancellationToken
        );

        if (solicitacao is null)
            return Result.Fail(ErrosDeSolicitacaoCertificado.NenhumaSolicitacaoEncontrada(query.CursoId));

        if (solicitacao.StatusSolicitacao != StatusSolicitacao.Concluido || solicitacao.CaminhoZip is null)
            return Result.Fail(ErrosDeSolicitacaoCertificado.ZipAindaNaoDisponivel(query.CursoId));

        var conteudo = await armazenamento.LerAsync(solicitacao.CaminhoZip, cancellationToken);

        return Result.Ok(new ArquivoDto(
            conteudo,
            $"certificados-{query.CursoId}.zip",
            "application/zip"
        ));
    }
}
