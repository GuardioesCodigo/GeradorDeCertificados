using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using GeradorCertificado.Infra.Compartilhado.Orm;
using Microsoft.EntityFrameworkCore;

namespace GeradorCertificado.Infra.Modulos.GeracaoCertificados.SolicitacaoCertificados;

public sealed class RepositorioSolicitacaoCertificadoEmOrm(
    GeradorCertificadoDbContext dbContext
) : RepositorioBaseEmOrm<SolicitacaoCertificado>(dbContext), IRepositorioSolicitacaoCertificado
{
    public override Task<SolicitacaoCertificado?> SelecionarPorIdAsync(
        Guid idSelecionado,
        CancellationToken cancellationToken = default
    )
    {
        return registros
            .Include(solicitacao => solicitacao.Certificados)
            .SingleOrDefaultAsync(
                solicitacao => solicitacao.Id == idSelecionado,
                cancellationToken
            );
    }

    public Task<SolicitacaoCertificado?> SelecionarEmProcessamentoPorCursoAsync(
        Guid cursoId,
        CancellationToken cancellationToken = default
    )
    {
        return registros
            .Include(solicitacao => solicitacao.Certificados)
            .SingleOrDefaultAsync(
                solicitacao =>
                    solicitacao.CursoId == cursoId &&
                    solicitacao.StatusSolicitacao != StatusSolicitacao.Concluido &&
                    solicitacao.StatusSolicitacao != StatusSolicitacao.Falha,
                cancellationToken
            );
    }

    public Task<SolicitacaoCertificado?> SelecionarMaisRecentePorCursoAsync(
        Guid cursoId,
        CancellationToken cancellationToken = default
    )
    {
        return registros
            .Include(solicitacao => solicitacao.Certificados)
            .Where(solicitacao => solicitacao.CursoId == cursoId)
            .OrderByDescending(solicitacao => solicitacao.DataSolicitacao)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task SalvarAsync(CancellationToken cancellationToken = default)
    {
        return SalvarAlteracoesAsync(cancellationToken);
    }
}
