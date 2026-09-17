using CursoEntidade = GeradorCertificado.Dominio.Modulos.Curso.Curso;
using GeradorCertificado.Dominio.Modulos.Curso;
using GeradorCertificado.Infra.Compartilhado.Orm;

namespace GeradorCertificado.Infra.Modulos.Curso;

public sealed class RepositorioCursoEmOrm(
    GeradorCertificadoDbContext dbContext
) : RepositorioBaseEmOrm<CursoEntidade>(dbContext), IRepositorioCurso
{
}