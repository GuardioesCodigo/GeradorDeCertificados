using GeradorCertificado.Dominio.Modulos.Cursos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeradorCertificado.UnitTests.Dominio;

[TestClass]
public sealed class CursoTests
{
    private static Curso CriarCursoValido()
    {
        return new Curso(
            Guid.CreateVersion7(),
            "Desenvolvimento Web com ASP.NET Core",
            "Curso completo de Web APIs com Clean Architecture.",
            40,
            new DateTime(2026, 12, 1)
        );
    }

    [TestMethod]
    public void Validar_DeveRetornarSemErros_QuandoCursoEhValido()
    {
        var curso = CriarCursoValido();

        var erros = curso.Validar();

        Assert.AreEqual(0, erros.Count);
    }

    [TestMethod]
    public void Validar_DeveRetornarErro_QuandoNomeEhVazio()
    {
        var curso = new Curso(Guid.CreateVersion7(), string.Empty, "descrição", 10, DateTime.Today);

        var erros = curso.Validar();

        Assert.IsTrue(erros.Any(erro => erro.Campo == nameof(Curso.Nome)));
    }

    [TestMethod]
    public void Validar_DeveRetornarErro_QuandoNomeExcede200Caracteres()
    {
        var curso = new Curso(Guid.CreateVersion7(), new string('A', 201), "descrição", 10, DateTime.Today);

        var erros = curso.Validar();

        Assert.IsTrue(erros.Any(erro => erro.Campo == nameof(Curso.Nome)));
    }

    [TestMethod]
    public void Validar_NaoDeveRetornarErro_QuandoDescricaoEhVazia()
    {
        var curso = new Curso(Guid.CreateVersion7(), "Curso sem descrição", string.Empty, 10, DateTime.Today);

        var erros = curso.Validar();

        Assert.IsFalse(erros.Any(erro => erro.Campo == nameof(Curso.Descricao)));
    }

    [TestMethod]
    public void Validar_DeveRetornarErro_QuandoDescricaoExcede500Caracteres()
    {
        var curso = new Curso(Guid.CreateVersion7(), "Curso", new string('B', 501), 10, DateTime.Today);

        var erros = curso.Validar();

        Assert.IsTrue(erros.Any(erro => erro.Campo == nameof(Curso.Descricao)));
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-5)]
    public void Validar_DeveRetornarErro_QuandoCargaHorariaNaoEhMaiorQueZero(int cargaHoraria)
    {
        var curso = new Curso(Guid.CreateVersion7(), "Curso", "descrição", cargaHoraria, DateTime.Today);

        var erros = curso.Validar();

        Assert.IsTrue(erros.Any(erro => erro.Campo == nameof(Curso.CargaHoraria)));
    }

    [TestMethod]
    public void Validar_DeveRetornarErro_QuandoDataConclusaoNaoInformada()
    {
        var curso = new Curso(Guid.CreateVersion7(), "Curso", "descrição", 10, default);

        var erros = curso.Validar();

        Assert.IsTrue(erros.Any(erro => erro.Campo == nameof(Curso.DataConclusao)));
    }

    [TestMethod]
    public void Atualizar_DeveSubstituirTodosOsCamposDoCurso()
    {
        var curso = CriarCursoValido();
        var atualizado = new Curso(Guid.CreateVersion7(), "Novo nome", "Nova descrição", 80, new DateTime(2027, 1, 1));

        curso.Atualizar(atualizado);

        Assert.AreEqual("Novo nome", curso.Nome);
        Assert.AreEqual("Nova descrição", curso.Descricao);
        Assert.AreEqual(80, curso.CargaHoraria);
        Assert.AreEqual(new DateTime(2027, 1, 1), curso.DataConclusao);
    }
}
