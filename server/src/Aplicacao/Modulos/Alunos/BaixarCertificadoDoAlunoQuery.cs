using FluentResults;
using GeradorCertificado.Aplicacao.Modulos.Alunos.Util;
using GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.DTOs;
using GeradorCertificado.Dominio.Modulos.Alunos;
using GeradorCertificado.Dominio.Modulos.Cursos;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Servicos;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using MediatR;

namespace GeradorCertificado.Aplicacao.Modulos.Alunos;

public sealed record BaixarCertificadoDoAlunoQuery(
    Guid AlunoId,
    Guid CertificadoId
) : IRequest<Result<ArquivoDto>>;

public sealed class BaixarCertificadoDoAlunoQueryHandler(
    IRepositorioAluno repositorioAluno,
    IRepositorioCertificado repositorioCertificado,
    IRepositorioSolicitacaoCertificado repositorioSolicitacao,
    IRepositorioCurso repositorioCurso,
    IArmazenamentoDeArquivos armazenamento
) : IRequestHandler<BaixarCertificadoDoAlunoQuery, Result<ArquivoDto>>
{
    public async Task<Result<ArquivoDto>> Handle(
        BaixarCertificadoDoAlunoQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var aluno = await repositorioAluno.SelecionarPorIdAsync(query.AlunoId, cancellationToken);

        if (aluno is null)
            return Result.Fail<ArquivoDto>(ErrosDeAluno.AlunoNaoEncontrado(query.AlunoId));

        var itens = await CertificadosDoAlunoCarregador.CarregarAsync(
            query.AlunoId,
            repositorioCertificado,
            repositorioSolicitacao,
            repositorioCurso,
            cancellationToken
        );

        var item = itens.FirstOrDefault(i => i.Certificado.Id == query.CertificadoId);

        if (item is null)
            return Result.Fail<ArquivoDto>(
                ErrosDeAluno.CertificadoNaoEncontrado(query.AlunoId, query.CertificadoId));

        byte[] pdf;

        try
        {
            pdf = await armazenamento.LerAsync(item.Certificado.CaminhoArquivo!, cancellationToken);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            return Result.Fail<ArquivoDto>(ErrosDeAluno.ArquivoIndisponivel(item.Certificado.Id));
        }

        string nomeArquivo =
            $"Certificado - {NomesDeArquivo.Sanitizar(item.Curso.Nome)} - {NomesDeArquivo.Sanitizar(aluno.Nome)}.pdf";

        return Result.Ok(new ArquivoDto(pdf, nomeArquivo, "application/pdf"));
    }
}
