namespace GeradorCertificado.Aplicacao.Modulos.Alunos.DTOs;

public sealed record AlunoDto(
    Guid Id,
    string Nome
);

public sealed record CertificadoDoAlunoDto(
    Guid CertificadoId,
    Guid CursoId,
    string CursoNome,
    int CargaHoraria,
    DateTime DataConclusao,
    DateTime? DataGeracao
);
