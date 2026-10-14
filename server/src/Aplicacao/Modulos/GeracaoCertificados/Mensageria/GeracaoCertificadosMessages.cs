namespace GeradorCertificado.Aplicacao.Modulos.GeracaoCertificados.Mensageria;

public sealed record GerarCertificadosMessage(
    Guid SolicitacaoId,
    Guid CursoId
);
