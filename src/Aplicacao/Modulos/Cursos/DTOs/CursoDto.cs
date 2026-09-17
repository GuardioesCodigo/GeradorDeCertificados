namespace GeradorCertificado.Aplicacao.Modulos.Cursos.DTOs;

public record CursoDto(
    Guid Id,
    string Nome,
    string Descricao,
    int CargaHoraria,
    DateTime DataConclusao
);