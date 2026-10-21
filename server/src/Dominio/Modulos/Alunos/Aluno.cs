using GeradorCertificado.Dominio.Compartilhado;

namespace GeradorCertificado.Dominio.Modulos.Alunos;

public sealed class Aluno : EntidadeBase<Aluno>
{
    public string Nome { get; private set; } = string.Empty;

    private Aluno() { }

    public Aluno(Guid id, string nome)
    {
        Id = id;
        Nome = nome;
    }

    /// <summary>Remove espaços nas pontas e reduz espaços repetidos entre as palavras.</summary>
    public static string NormalizarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return string.Empty;

        return string.Join(' ', nome.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    public override IReadOnlyList<ErroValidacao> Validar()
    {
        List<ErroValidacao> erros = [];

        if (string.IsNullOrWhiteSpace(Nome))
            erros.Add(new(nameof(Nome), "O campo \"Nome\" do Aluno é obrigatório."));

        else if (Nome.Length > 200)
            erros.Add(new(nameof(Nome), "O \"nome\" do Aluno deve possuir no máximo 200 caracteres."));

        return erros;
    }

    public override void Atualizar(Aluno entidadeAtualizada)
    {
        Nome = entidadeAtualizada.Nome;
    }
}
