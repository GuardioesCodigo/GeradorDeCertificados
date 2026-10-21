using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeradorCertificado.Infra.Compartilhado.Orm.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarAlunos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AlunoId",
                table: "TBCertificados",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TBAlunos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBAlunos", x => x.Id);
                });

            // Aproveita os certificados já emitidos: cada nome distinto vira um aluno
            // e os certificados existentes passam a apontar para ele.
            migrationBuilder.Sql(
                @"INSERT INTO TBAlunos (Id, Nome)
                  SELECT NEWID(), n.Nome
                  FROM (SELECT DISTINCT LTRIM(RTRIM(NomeAluno)) AS Nome FROM TBCertificados) AS n
                  WHERE n.Nome <> '';");

            migrationBuilder.Sql(
                @"UPDATE c
                  SET c.AlunoId = a.Id
                  FROM TBCertificados AS c
                  INNER JOIN TBAlunos AS a ON a.Nome = LTRIM(RTRIM(c.NomeAluno));");

            migrationBuilder.CreateIndex(
                name: "IX_TBCertificados_AlunoId",
                table: "TBCertificados",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_TBAlunos_Nome",
                table: "TBAlunos",
                column: "Nome",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TBCertificados_TBAlunos_AlunoId",
                table: "TBCertificados",
                column: "AlunoId",
                principalTable: "TBAlunos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBCertificados_TBAlunos_AlunoId",
                table: "TBCertificados");

            migrationBuilder.DropTable(
                name: "TBAlunos");

            migrationBuilder.DropIndex(
                name: "IX_TBCertificados_AlunoId",
                table: "TBCertificados");

            migrationBuilder.DropColumn(
                name: "AlunoId",
                table: "TBCertificados");
        }
    }
}
