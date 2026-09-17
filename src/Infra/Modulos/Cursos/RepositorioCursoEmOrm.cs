using CursoEntidade = GeradorCertificado.Dominio.Modulos.Cursos.Curso;
using GeradorCertificado.Dominio.Modulos.Cursos;
using GeradorCertificado.Infra.Compartilhado.Orm;

namespace GeradorCertificado.Infra.Modulos.Curso;

public sealed class RepositorioCursoEmOrm(
    GeradorCertificadoDbContext dbContext
) : RepositorioBaseEmOrm<CursoEntidade>(dbContext), IRepositorioCurso
{
}