using GeradorCertificado.Dominio.Compartilhado;

namespace GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;

public interface IRepositorioSolicitacaoCertificado : IRepositorio<SolicitacaoCertificado>
{
    Task<SolicitacaoCertificado?> SelecionarEmProcessamentoPorCursoAsync(
        Guid cursoId,
        CancellationToken cancellationToken = default
    );

    Task<SolicitacaoCertificado?> SelecionarMaisRecentePorCursoAsync(
        Guid cursoId,
        CancellationToken cancellationToken = default
    );

    Task SalvarAsync(
        CancellationToken cancellationToken = default
    );
}
