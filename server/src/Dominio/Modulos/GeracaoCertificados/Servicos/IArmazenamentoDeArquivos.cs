namespace GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Servicos;

public interface IArmazenamentoDeArquivos
{
    Task<string> SalvarAsync(
        string caminhoRelativo,
        byte[] conteudo,
        CancellationToken cancellationToken = default
    );

    Task<byte[]> LerAsync(
        string caminhoRelativo,
        CancellationToken cancellationToken = default
    );
}
