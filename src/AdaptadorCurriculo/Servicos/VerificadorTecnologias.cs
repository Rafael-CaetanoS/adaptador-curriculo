using System.Text.RegularExpressions;
using AdaptadorCurriculo.Configuracao;

namespace AdaptadorCurriculo.Servicos;

/// <summary>
/// Trava "não inventar": encontra tecnologias (de prompts/tecnologias.txt) que aparecem no currículo
/// gerado mas não têm base no currículo original nem no perfil extra.
/// </summary>
public sealed partial class VerificadorTecnologias(Caminhos caminhos)
{
    private sealed record Termo(string Nome, Regex Busca, IReadOnlyList<Regex> Implicadores);

    public IReadOnlyList<string> SemBase(string texGerado, string fontePermitida)
    {
        var gerado = Normalizar(texGerado);
        var fonte = Normalizar(fontePermitida);
        return CarregarTermos()
            .Where(t => t.Busca.IsMatch(gerado) && !t.Busca.IsMatch(fonte) && !t.Implicadores.Any(i => i.IsMatch(fonte)))
            .Select(t => t.Nome)
            .ToList();
    }

    /// <summary>Texto que pode servir de base: currículo original (sem comentários %) + perfil extra (sem comentários HTML).</summary>
    public static string MontarFonte(string curriculoBase, string perfilExtra) =>
        ComentarioLatex().Replace(curriculoBase, "") + "\n" + ComentarioHtml().Replace(perfilExtra, "");

    private List<Termo> CarregarTermos() =>
        File.ReadLines(caminhos.Prompt("tecnologias.txt"))
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith('#'))
            .Select(l =>
            {
                var partes = l.Split("<=", 2);
                var implicadores = partes.Length > 1
                    ? partes[1].Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(Palavra).ToList()
                    : [];
                var nome = partes[0].Trim();
                return new Termo(nome, Palavra(nome), implicadores);
            })
            .ToList();

    // Palavra inteira, sem confundir com letras/números vizinhos (ex.: "Git" não casa em "GitHub")
    private static Regex Palavra(string termo) =>
        new($@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(termo)}(?![\p{{L}}\p{{N}}#+])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    // Desfaz escapes do LaTeX (C\# → C#) e tira comentários
    private static string Normalizar(string texto) =>
        ComentarioLatex().Replace(texto, "").Replace(@"\#", "#").Replace(@"\&", "&").Replace(@"\_", "_").Replace("~", " ");

    [GeneratedRegex(@"(?<!\\)%.*$", RegexOptions.Multiline)]
    private static partial Regex ComentarioLatex();

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex ComentarioHtml();
}
