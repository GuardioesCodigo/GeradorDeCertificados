using GeradorCertificado.Dominio.Modulos.Alunos;
using GeradorCertificado.Infra.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;

namespace GeradorCertificado.Infra.Modulos.Alunos;

public sealed class RepositorioAlunoEmOrm(
    GeradorCertificadoDbContext dbContext
) : RepositorioBaseEmOrm<Aluno>(dbContext), IRepositorioAluno
{
    public Task<Aluno?> SelecionarPorNomeAsync(
        string nome,
        CancellationToken cancellationToken = default
    )
    {
        string nomeNormalizado = nome.ToLower();

        return registros.FirstOrDefaultAsync(
            aluno => aluno.Nome.ToLower() == nomeNormalizado,
            cancellationToken
        );
    }

    public Task<List<Aluno>> BuscarPorNomeAsync(
        string? trecho,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Aluno> consulta = registros;

        if (!string.IsNullOrWhiteSpace(trecho))
        {
            string trechoNormalizado = trecho.Trim().ToLower();

            consulta = consulta.Where(aluno => aluno.Nome.ToLower().Contains(trechoNormalizado));
        }

        return consulta
            .OrderBy(aluno => aluno.Nome)
            .Take(200)
            .ToListAsync(cancellationToken);
    }
}
