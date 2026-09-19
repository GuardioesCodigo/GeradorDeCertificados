namespace GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.DTOs;

public sealed record CertificadoDto(
    Guid Id,
    string NomeAluno,
    string Status,
    string? CaminhoArquivo,
    DateTime? DataGeracao
);

public sealed record StatusSolicitacaoDto(
    Guid SolicitacaoId,
    Guid CursoId,
    string Status,
    DateTime DataSolicitacao,
    int TotalCertificados,
    int CertificadosGerados,
    int CertificadosComFalha
);

public sealed record ArquivoDto(
    byte[] Conteudo,
    string NomeArquivo,
    string ContentType
);
