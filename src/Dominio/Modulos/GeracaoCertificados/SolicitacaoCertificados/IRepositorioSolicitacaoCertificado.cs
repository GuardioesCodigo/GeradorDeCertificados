using GeradorCertificado.Dominio.Compartilhado;

namespace GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;

public interface IRepositorioSolicitacaoCertificados : IRepositorio<SolicitacaoCertificado>
{
    Task<SolicitacaoCertificado?> SelecionarEmProcessamentoPorCursoAsync(
        Guid cursoId
    );
}