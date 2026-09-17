using GeradorCertificado.Dominio.Compartilhado;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;

namespace GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;

public sealed class SolicitacaoCertificado : EntidadeBase<SolicitacaoCertificado>
{
    public Guid CursoId { get; private set; }
    public List<Certificado> Certificados { get; private set; } = [];
    public StatusSolicitacao StatusSolicitacao { get; private set; }
    public DateTime DataSolicitacao { get; private set; } = DateTime.Now;

    private SolicitacaoCertificado() { }

    public SolicitacaoCertificado(Guid cursoId, List<Certificado> certificados)
    {
        CursoId = cursoId;
        Certificados = certificados;
        StatusSolicitacao = StatusSolicitacao.Pendente;
    }

    public override IReadOnlyList<ErroValidacao> Validar()
    {
        List<ErroValidacao> erros = [];

        if (CursoId == Guid.Empty)
            erros.Add(new(nameof(CursoId), "O Curso é obrigatório."));

        if (Certificados.Count == 0)
            erros.Add(new(nameof(Certificados), "A solicitação deve possuir pelo menos um aluno."));

        return erros;
    }

    public override void Atualizar(SolicitacaoCertificado entidadeAtualizada)
    {
        CursoId = entidadeAtualizada.CursoId;
        Certificados = entidadeAtualizada.Certificados;
        StatusSolicitacao = entidadeAtualizada.StatusSolicitacao;
        DataSolicitacao = entidadeAtualizada.DataSolicitacao;
    }
}