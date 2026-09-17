using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using GeradorCertificado.Infra.Orm;
using Microsoft.EntityFrameworkCore;

namespace GeradorCertificado.Infra.Modulos.GeracaoCertificados.SolicitacaoCertificados;

public sealed class RepositorioSolicitacaoCertificadoEmOrm(
    GeradorCertificadoDbContext dbContext
) : RepositorioBaseEmOrm<SolicitacaoCertificado>(dbContext), IRepositorioSolicitacaoCertificados
{
    public Task<SolicitacaoCertificado?> SelecionarEmProcessamentoPorCursoAsync(
        Guid cursoId
    )
    {
        return registros.SingleOrDefaultAsync(
            solicitacao =>
                solicitacao.CursoId == cursoId &&
                solicitacao.StatusSolicitacao != StatusSolicitacao.Concluido &&
                solicitacao.StatusSolicitacao != StatusSolicitacao.Falha
        );
    }
}