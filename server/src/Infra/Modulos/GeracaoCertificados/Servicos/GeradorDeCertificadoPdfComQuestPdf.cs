using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Servicos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GeradorCertificado.Infra.Modulos.GeracaoCertificados.Servicos;

public sealed class GeradorDeCertificadoPdfComQuestPdf : IGeradorDeCertificadoPdf
{
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
                page.DefaultTextStyle(x => x.FontSize(14));

                page.Content()
                    .Border(2)
                    .BorderColor(Colors.Blue.Darken2)
                    .Padding(40)
                    .Column(column =>
                    {
                        column.Spacing(20);

                        column.Item().AlignCenter().Text("Certificado de Conclusão")
                            .FontSize(30).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(20).AlignCenter()
                            .Text("Certificamos que").FontSize(16);

                        column.Item().AlignCenter()
                            .Text(nomeAluno).FontSize(24).Bold();

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
