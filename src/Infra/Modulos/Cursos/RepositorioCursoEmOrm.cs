using GeradorCertificado.Dominio.Modulos.Cursos;
using GeradorCertificado.Infra.Compartilhado.Orm;

namespace GeradorCertificado.Infra.Modulos.Cursos;

public sealed class RepositorioCursoEmOrm(
    GeradorCertificadoDbContext dbContext
) : RepositorioBaseEmOrm<Curso>(dbContext), IRepositorioCurso
{
}
