namespace GeradorCertificado.Api.Modulos.Alunos;

public sealed record AlunoResponse(
    Guid Id,
    string Nome
);

public sealed record CertificadoDoAlunoResponse(
    Guid CertificadoId,
    Guid CursoId,
    string CursoNome,
    int CargaHoraria,
    DateTime DataConclusao,
    DateTime? DataGeracao
);

/// <summary>Ids dos certificados desejados; lista vazia ou ausente baixa todos os do aluno.</summary>
public sealed record BaixarCertificadosDoAlunoRequest(
    List<Guid>? CertificadoIds
);
