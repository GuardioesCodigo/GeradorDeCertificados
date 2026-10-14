using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.Certificados;
using GeradorCertificado.Dominio.Modulos.GeracaoCertificados.SolicitacaoCertificados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeradorCertificado.Infra.Compartilhado.Orm.Config;

public sealed class SolicitacaoCertificadoConfiguration : IEntityTypeConfiguration<SolicitacaoCertificado>
{
    public void Configure(EntityTypeBuilder<SolicitacaoCertificado> builder)
    {
        builder.ToTable("TBSolicitacoesCertificados");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.CursoId).IsRequired();

        builder.Property(e => e.StatusSolicitacao)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.DataSolicitacao).IsRequired();

        builder.Property(e => e.CaminhoZip).HasMaxLength(500);

        builder.HasMany(e => e.Certificados)
            .WithOne()
            .HasForeignKey(c => c.SolicitacaoCertificadoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CertificadoConfiguration : IEntityTypeConfiguration<Certificado>
{
    public void Configure(EntityTypeBuilder<Certificado> builder)
    {
        builder.ToTable("TBCertificados");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.NomeAluno)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.CaminhoArquivo).HasMaxLength(500);

        builder.Property(e => e.SolicitacaoCertificadoId).IsRequired();
    }
}
