using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Servicos;
using Microsoft.Extensions.Configuration;

namespace GeradorCertificado.Infra.Modulos.GeracaoCertificados.Servicos;

public sealed class ArmazenamentoDeArquivosEmDisco : IArmazenamentoDeArquivos
{
    private readonly string caminhoBase;

    public ArmazenamentoDeArquivosEmDisco(IConfiguration configuration)
    {
        caminhoBase = configuration["Armazenamento:CaminhoBase"] is string caminhoConfigurado
            && !string.IsNullOrWhiteSpace(caminhoConfigurado)
                ? caminhoConfigurado
                : Path.Combine(AppContext.BaseDirectory, "Arquivos");
    }

    public async Task<string> SalvarAsync(
        string caminhoRelativo,
        byte[] conteudo,
        CancellationToken cancellationToken = default
    )
    {
        string caminhoCompleto = ResolverCaminhoCompleto(caminhoRelativo);

        string? diretorio = Path.GetDirectoryName(caminhoCompleto);

        if (!string.IsNullOrEmpty(diretorio))
            Directory.CreateDirectory(diretorio);

        await File.WriteAllBytesAsync(caminhoCompleto, conteudo, cancellationToken);

        return caminhoRelativo;
    }

    public async Task<byte[]> LerAsync(
        string caminhoRelativo,
        CancellationToken cancellationToken = default
    )
    {
        string caminhoCompleto = ResolverCaminhoCompleto(caminhoRelativo);

        return await File.ReadAllBytesAsync(caminhoCompleto, cancellationToken);
    }

    private string ResolverCaminhoCompleto(string caminhoRelativo)
    {
        string caminhoNormalizado = caminhoRelativo.Replace('/', Path.DirectorySeparatorChar);

        return Path.Combine(caminhoBase, caminhoNormalizado);
    }
}
