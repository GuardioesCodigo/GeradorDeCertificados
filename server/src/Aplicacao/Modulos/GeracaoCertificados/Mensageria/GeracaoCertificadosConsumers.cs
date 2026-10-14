using System.IO.Compression;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Servicos;
using GeradorCertificado.Dominio.Modulos.Cursos;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Mensageria;

public sealed class GerarCertificadosConsumer(
    IRepositorioSolicitacaoCertificado repositorioSolicitacao,
    IRepositorioCurso repositorioCurso,
    IGeradorDeCertificadoPdf geradorDeCertificadoPdf,
    IArmazenamentoDeArquivos armazenamento,
    ILogger<GerarCertificadosConsumer> logger
) : IConsumer<GerarCertificadosMessage>
{
    public async Task Consume(ConsumeContext<GerarCertificadosMessage> context)
    {
        var mensagem = context.Message;

        var solicitacao = await repositorioSolicitacao.SelecionarPorIdAsync(
            mensagem.SolicitacaoId,
            context.CancellationToken
        );

        if (solicitacao is null)
        {
            logger.LogWarning(
                "A solicitação {SolicitacaoId} não foi encontrada para processamento.",
                mensagem.SolicitacaoId
            );

            return;
        }

        if (solicitacao.StatusSolicitacao != StatusSolicitacao.Pendente)
        {
            logger.LogInformation(
                "A solicitação {SolicitacaoId} já foi processada anteriormente.",
                mensagem.SolicitacaoId
            );

            return;
        }

        var curso = await repositorioCurso.SelecionarPorIdAsync(mensagem.CursoId, context.CancellationToken);

        if (curso is null)
        {
            logger.LogWarning(
                "O curso {CursoId} não foi encontrado ao processar a solicitação {SolicitacaoId}.",
                mensagem.CursoId,
                mensagem.SolicitacaoId
            );

            solicitacao.Falhar();

            await repositorioSolicitacao.SalvarAsync(context.CancellationToken);

            return;
        }

        solicitacao.IniciarProcessamento();

        await repositorioSolicitacao.SalvarAsync(context.CancellationToken);

        foreach (Certificado certificado in solicitacao.Certificados)
        {
            try
            {
                byte[] pdf = geradorDeCertificadoPdf.Gerar(
                    certificado.NomeAluno,
                    curso.Nome,
                    curso.CargaHoraria,
                    curso.DataConclusao
                );

                string caminhoRelativo = $"certificados/{solicitacao.Id}/{certificado.Id}.pdf";

                await armazenamento.SalvarAsync(caminhoRelativo, pdf, context.CancellationToken);

                certificado.MarcarComoGerado(caminhoRelativo, DateTime.Now);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Falha ao gerar o certificado {CertificadoId} da solicitação {SolicitacaoId}.",
                    certificado.Id,
                    solicitacao.Id
                );

                certificado.MarcarComoFalha();
            }
        }

        await repositorioSolicitacao.SalvarAsync(context.CancellationToken);

        bool nenhumCertificadoGerado = solicitacao.Certificados.All(c => c.Status == StatusCertificado.Falha);

        if (nenhumCertificadoGerado)
        {
            logger.LogWarning(
                "Nenhum certificado pôde ser gerado para a solicitação {SolicitacaoId}.",
                solicitacao.Id
            );

            solicitacao.Falhar();

            await repositorioSolicitacao.SalvarAsync(context.CancellationToken);

            return;
        }

        solicitacao.IniciarGeracaoZip();

        await repositorioSolicitacao.SalvarAsync(context.CancellationToken);

        try
        {
            byte[] zip = await MontarZipAsync(solicitacao, armazenamento, context.CancellationToken);

            string caminhoZip = $"certificados/{solicitacao.Id}/certificados.zip";

            await armazenamento.SalvarAsync(caminhoZip, zip, context.CancellationToken);

            solicitacao.Concluir(caminhoZip);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha ao montar o arquivo ZIP da solicitação {SolicitacaoId}.",
                solicitacao.Id
            );

            solicitacao.Falhar();
        }

        await repositorioSolicitacao.SalvarAsync(context.CancellationToken);
    }

    private static async Task<byte[]> MontarZipAsync(
        SolicitacaoCertificado solicitacao,
        IArmazenamentoDeArquivos armazenamento,
        CancellationToken cancellationToken
    )
    {
        using MemoryStream zipStream = new();

        using (ZipArchive zip = new(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var certificadosGerados = solicitacao.Certificados
                .Where(c => c.Status == StatusCertificado.Gerado && c.CaminhoArquivo is not null);

            foreach (Certificado certificado in certificadosGerados)
            {
                byte[] pdf = await armazenamento.LerAsync(certificado.CaminhoArquivo!, cancellationToken);

                string nomeArquivo = $"{certificado.NomeAluno} - {certificado.Id.ToString()[..8]}.pdf";

                ZipArchiveEntry entrada = zip.CreateEntry(nomeArquivo, CompressionLevel.Fastest);

                await using Stream entradaStream = entrada.Open();
                await entradaStream.WriteAsync(pdf, cancellationToken);
            }
        }

        return zipStream.ToArray();
    }
}
