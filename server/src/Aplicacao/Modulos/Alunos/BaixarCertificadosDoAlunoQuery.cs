using System.IO.Compression;
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

/// <summary>Monta um ZIP com os certificados informados; sem ids, inclui todos os do aluno.</summary>
public sealed record BaixarCertificadosDoAlunoQuery(
    Guid AlunoId,
    IReadOnlyList<Guid> CertificadoIds
) : IRequest<Result<ArquivoDto>>;

public sealed class BaixarCertificadosDoAlunoQueryHandler(
    IRepositorioAluno repositorioAluno,
    IRepositorioCertificado repositorioCertificado,
    IRepositorioSolicitacaoCertificado repositorioSolicitacao,
    IRepositorioCurso repositorioCurso,
    IArmazenamentoDeArquivos armazenamento
) : IRequestHandler<BaixarCertificadosDoAlunoQuery, Result<ArquivoDto>>
{
    public async Task<Result<ArquivoDto>> Handle(
        BaixarCertificadosDoAlunoQuery query,
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

        List<CertificadoDoAluno> selecionados = itens;

        if (query.CertificadoIds.Count > 0)
        {
            HashSet<Guid> idsSolicitados = [.. query.CertificadoIds];

            selecionados = itens
                .Where(item => idsSolicitados.Contains(item.Certificado.Id))
                .ToList();

            foreach (Guid idSolicitado in idsSolicitados)
            {
                if (!selecionados.Any(item => item.Certificado.Id == idSolicitado))
                    return Result.Fail<ArquivoDto>(
                        ErrosDeAluno.CertificadoNaoEncontrado(query.AlunoId, idSolicitado));
            }
        }

        if (selecionados.Count == 0)
            return Result.Fail<ArquivoDto>(ErrosDeAluno.NenhumCertificadoDisponivel(query.AlunoId));

        byte[] zip;

        try
        {
            zip = await MontarZipAsync(selecionados, cancellationToken);
        }
        catch (ArquivoDoCertificadoIndisponivelException ex)
        {
            return Result.Fail<ArquivoDto>(ErrosDeAluno.ArquivoIndisponivel(ex.CertificadoId));
        }

        string nomeZip = $"certificados-{NomesDeArquivo.Sanitizar(aluno.Nome)}.zip";

        return Result.Ok(new ArquivoDto(zip, nomeZip, "application/zip"));
    }

    private async Task<byte[]> MontarZipAsync(
        List<CertificadoDoAluno> itens,
        CancellationToken cancellationToken
    )
    {
        using MemoryStream zipStream = new();

        using (ZipArchive zip = new(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            HashSet<string> nomesUsados = new(StringComparer.OrdinalIgnoreCase);

            foreach (CertificadoDoAluno item in itens)
            {
                byte[] pdf;

                try
                {
                    pdf = await armazenamento.LerAsync(
                        item.Certificado.CaminhoArquivo!,
                        cancellationToken
                    );
                }
                catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
                {
                    throw new ArquivoDoCertificadoIndisponivelException(item.Certificado.Id, ex);
                }

                string nomeBase = NomesDeArquivo.Sanitizar(item.Curso.Nome);
                string nomeArquivo = $"{nomeBase}.pdf";
                int sufixo = 2;

                while (!nomesUsados.Add(nomeArquivo))
                {
                    nomeArquivo = $"{nomeBase} ({sufixo}).pdf";
                    sufixo++;
                }

                ZipArchiveEntry entrada = zip.CreateEntry(nomeArquivo, CompressionLevel.Fastest);

                await using Stream entradaStream = entrada.Open();
                await entradaStream.WriteAsync(pdf, cancellationToken);
            }
        }

        return zipStream.ToArray();
    }
}

internal sealed class ArquivoDoCertificadoIndisponivelException(
    Guid certificadoId,
    Exception innerException
) : Exception("O arquivo do certificado não está disponível.", innerException)
{
    public Guid CertificadoId { get; } = certificadoId;
}
