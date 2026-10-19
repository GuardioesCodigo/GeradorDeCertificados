using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Servicos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GeradorCertificado.Infra.Modulos.GeracaoCertificados.Servicos;

public sealed class GeradorDeCertificadoPdfComQuestPdf : IGeradorDeCertificadoPdf
{
    private const string CorFundo = "#8FA0B5";   // azul-marinho acinzentado
    private const string CorBorda = "#1B2A49";   // azul-marinho escuro

    static GeradorDeCertificadoPdfComQuestPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Gerar(
        string nomeAluno,
        string nomeCurso,
        int cargaHorariaCurso,
        DateTime dataConclusaoCurso
    )
    {
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.PageColor(CorFundo);
                page.DefaultTextStyle(x => x.FontSize(14).Bold().FontColor(Colors.Black));

                page.Content()
                    .Border(2)
                    .BorderColor(CorBorda)
                    .Padding(40)
                    .Column(column =>
                    {
                        column.Spacing(20);

                        column.Item().AlignCenter().Text("Certificado de Conclusão")
                            .FontSize(30).Bold().FontColor(Colors.Black);

                        column.Item().PaddingTop(20).AlignCenter()
                            .Text("Certificamos que").FontSize(16);

                        column.Item().AlignCenter()
                            .Text(nomeAluno).FontSize(24).Bold().FontColor(Colors.Black);

                        column.Item().AlignCenter()
                            .Text($"concluiu o curso \"{nomeCurso}\", com carga horária de " +
                                  $"{cargaHorariaCurso} horas, em {dataConclusaoCurso:dd/MM/yyyy}.")
                            .FontSize(16);
                    });
            });
        });

        return documento.GeneratePdf();
    }
}
