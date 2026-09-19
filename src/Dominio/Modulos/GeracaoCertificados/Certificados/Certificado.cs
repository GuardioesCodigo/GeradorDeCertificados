using System;
using GeradorCertificado.Dominio.Compartilhado;

namespace GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;

public sealed class Certificado : EntidadeBase<Certificado>
{
    public string NomeAluno { get; private set; } = string.Empty;
    public StatusCertificado Status { get; private set; }
    public Guid SolicitacaoCertificadoId { get; private set; }
    public string? CaminhoArquivo { get; private set; }
    public DateTime? DataGeracao { get; private set; }

    public Certificado() {}

    public Certificado(Guid id, string nomeAluno, StatusCertificado status, string? caminhoArquivo, DateTime? dataGeracao, Guid solicitacaoCertificadoId)
    {
        Id = id;
        NomeAluno = nomeAluno;
        Status = status;
        CaminhoArquivo = caminhoArquivo;
        DataGeracao = dataGeracao;
        SolicitacaoCertificadoId = solicitacaoCertificadoId;
    }

    public override IReadOnlyList<ErroValidacao> Validar()
    {
        List<ErroValidacao> erros = [];

        if (string.IsNullOrWhiteSpace(NomeAluno))
            erros.Add(new(nameof(NomeAluno), "O campo \"Nome do Aluno\" é obrigatório."));

        else if (NomeAluno.Length > 200)
            erros.Add(new(nameof(NomeAluno), "O \"nome do aluno\" deve possuir no máximo 200 caracteres."));

        return erros;
    }

    public override void Atualizar(Certificado entidadeAtualizada)
    {
        NomeAluno = entidadeAtualizada.NomeAluno;
        Status = entidadeAtualizada.Status;
        CaminhoArquivo = entidadeAtualizada.CaminhoArquivo;
        DataGeracao = entidadeAtualizada.DataGeracao;
    }

    public void MarcarComoGerado(string caminhoArquivo, DateTime dataGeracao)
    {
        Status = StatusCertificado.Gerado;
        CaminhoArquivo = caminhoArquivo;
        DataGeracao = dataGeracao;
    }

    public void MarcarComoFalha()
    {
        Status = StatusCertificado.Falha;
        CaminhoArquivo = null;
        DataGeracao = null;
    }

}
