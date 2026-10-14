namespace GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Servicos;

public interface IGeradorDeCertificadoPdf
{
    byte[] Gerar(
        string nomeAluno,
        string nomeCurso,
        int cargaHorariaCurso,
        DateTime dataConclusaoCurso
    );
}
