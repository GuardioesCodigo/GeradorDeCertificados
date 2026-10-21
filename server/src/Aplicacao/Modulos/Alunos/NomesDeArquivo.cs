namespace GeradorCertificado.Aplicacao.Modulos.Alunos;

internal static class NomesDeArquivo
{
    private static readonly char[] CaracteresInvalidos =
        ['\\', '/', ':', '*', '?', '"', '<', '>', '|', '\0'];

    public static string Sanitizar(string nome)
    {
        string limpo = new(nome
            .Select(caractere => Array.IndexOf(CaracteresInvalidos, caractere) >= 0 ? '_' : caractere)
            .ToArray());

        limpo = limpo.Trim();

        return limpo.Length == 0 ? "arquivo" : limpo;
    }
}
