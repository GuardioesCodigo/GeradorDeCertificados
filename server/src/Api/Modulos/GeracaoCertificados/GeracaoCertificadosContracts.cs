namespace GeradorCertificado.Api.Modulos.GeracaoCertificados;

public sealed record AlunoRequest(
    string Nome
);

public sealed record SolicitarGeracaoCertificadosRequest(
    List<AlunoRequest> Alunos
);

public sealed record SolicitarGeracaoCertificadosResponse(
    Guid SolicitacaoId,
    Guid CursoId
);

public sealed record StatusSolicitacaoResponse(
    Guid SolicitacaoId,
    Guid CursoId,
    string Status,
    DateTime DataSolicitacao,
    int TotalCertificados,
    int CertificadosGerados,
    int CertificadosComFalha
);

public sealed record CertificadoResponse(
    Guid Id,
    string NomeAluno,
    string Status,
    string? CaminhoArquivo,
    DateTime? DataGeracao
);
