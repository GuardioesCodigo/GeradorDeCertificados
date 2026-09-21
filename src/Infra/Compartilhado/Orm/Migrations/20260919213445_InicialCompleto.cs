using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeradorCertificado.Infra.Compartilhado.Orm.Migrations
{
    /// <inheritdoc />
    public partial class InicialCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBSolicitacoesCertificados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusSolicitacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataSolicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CaminhoZip = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBSolicitacoesCertificados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBCertificados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeAluno = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SolicitacaoCertificadoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaminhoArquivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataGeracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBCertificados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBCertificados_TBSolicitacoesCertificados_SolicitacaoCertif~",
                        column: x => x.SolicitacaoCertificadoId,
                        principalTable: "TBSolicitacoesCertificados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBCertificados_SolicitacaoCertificadoId",
                table: "TBCertificados",
                column: "SolicitacaoCertificadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBCertificados");

            migrationBuilder.DropTable(
                name: "TBSolicitacoesCertificados");
        }
    }
}
