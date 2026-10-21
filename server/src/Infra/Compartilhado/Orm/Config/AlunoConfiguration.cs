using GeradorCertificado.Dominio.Modulos.Alunos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeradorCertificado.Infra.Compartilhado.Orm.Config;

public sealed class AlunoConfiguration : IEntityTypeConfiguration<Aluno>
{
    public void Configure(EntityTypeBuilder<Aluno> builder)
    {
        builder.ToTable("TBAlunos");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Nome)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(e => e.Nome).IsUnique();
    }
}
