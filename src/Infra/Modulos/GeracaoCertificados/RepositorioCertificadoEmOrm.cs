using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Infra.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;

namespace GeradorCertificado.Infra.Modulos.GeracaoCertificados.Certificados;

public sealed class RepositorioCertificadoEmOrm(
    GeradorCertificadoDbContext dbContext
) : RepositorioBaseEmOrm<Certificado>(dbContext), IRepositorioCertificado
{
    public Task<List<Certificado>> SelecionarPorSolicitacaoAsync(
        Guid solicitacaoCertificadoId
    )
    {
        return registros
            .Where(certificado =>
                certificado.SolicitacaoCertificadoId == solicitacaoCertificadoId)
            .ToListAsync();
    }
}