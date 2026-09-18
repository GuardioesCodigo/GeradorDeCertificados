namespace GeradorCertificado.Api.Modulos.Cursos;

public sealed record CadastrarCursoRequest(
    string Nome,
    string? Descricao,
    int CargaHoraria,
    DateTime DataConclusao
);

public sealed record CadastrarCursoResponse(
    Guid Id,
    string Nome
);

public sealed record CursoResponse(
    Guid Id,
    string Nome,
    string Descricao,
    int CargaHoraria,
    DateTime DataConclusao
);
