using GeradorCertificado.Dominio.Modulos.Cursos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeradorCertificado.Infra.Compartilhado.Orm.Config;

public sealed class CursoConfiguration : IEntityTypeConfiguration<Curso>
{
    public void Configure(EntityTypeBuilder<Curso> builder)
    {
        builder.ToTable("TBCursos");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Nome)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Descricao)
            .HasMaxLength(500);

        builder.Property(e => e.CargaHoraria)
            .IsRequired();

        builder.Property(e => e.DataConclusao)
            .IsRequired();
    }
}
