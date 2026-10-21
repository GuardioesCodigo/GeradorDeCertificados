using GeradorCertificado.Dominio.Compartilhado;

namespace GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;

public interface IRepositorioCertificado : IRepositorio<Certificado>
{
    Task<List<Certificado>> SelecionarPorSolicitacaoAsync(
        Guid solicitacaoCertificadoId
    );

    Task<List<Certificado>> SelecionarGeradosPorAlunoAsync(
        Guid alunoId,
        CancellationToken cancellationToken = default
    );
}