using GeradorCertificado.Dominio.Compartilhado;

namespace GeradorCertificado.Dominio.Modulos.Alunos;

public interface IRepositorioAluno : IRepositorio<Aluno>
{
    Task<Aluno?> SelecionarPorNomeAsync(
        string nome,
        CancellationToken cancellationToken = default
    );

    Task<List<Aluno>> BuscarPorNomeAsync(
        string? trecho,
        CancellationToken cancellationToken = default
    );
}
