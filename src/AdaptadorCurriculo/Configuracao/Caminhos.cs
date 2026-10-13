namespace AdaptadorCurriculo.Configuracao;

/// <summary>
/// Onde ficam os dados do usuário (currículo base, perfil, saídas) e os prompts.
/// No Docker, DADOS_DIR aponta para a pasta do projeto montada em /dados.
/// Rodando local (dotnet run), procura a pasta que tem o curriculo-base.tex subindo a partir do executável.
/// </summary>
public sealed class Caminhos
{
    public string Dados { get; }
    public string CurriculoBase => Path.Combine(Dados, "curriculo-base.tex");
    public string PerfilExtra => Path.Combine(Dados, "perfil-extra.md");
    public string Saidas => Path.Combine(Dados, "saidas");
    public string Entradas => Path.Combine(Dados, "entradas");
    public string PreviewBase => Path.Combine(Dados, ".preview", "base");

    public Caminhos(IConfiguration config, IWebHostEnvironment env)
    {
        Dados = Path.GetFullPath(config["DADOS_DIR"] ?? ProcurarPastaDados(env.ContentRootPath));
        Directory.CreateDirectory(Saidas);
        Directory.CreateDirectory(Entradas);
    }

    /// <summary>Prompt editável na pasta de dados; se não existir, usa o que veio na imagem.</summary>
    public string Prompt(string nome)
    {
        var editavel = Path.Combine(Dados, "prompts", nome);
        return File.Exists(editavel) ? editavel : Path.Combine(AppContext.BaseDirectory, "prompts", nome);
    }

    /// <summary>Resolve um caminho relativo (ex.: "saidas/2026-...") garantindo que fica dentro de saidas/.</summary>
    public string? PastaDeSaida(string? relativo)
    {
        if (string.IsNullOrWhiteSpace(relativo)) return null;
        var abs = Path.GetFullPath(Path.Combine(Dados, relativo));
        var raiz = Path.GetFullPath(Saidas) + Path.DirectorySeparatorChar;
        return abs.StartsWith(raiz, StringComparison.Ordinal) && Directory.Exists(abs) ? abs : null;
    }

    public string Relativo(string abs) => Path.GetRelativePath(Dados, abs).Replace('\\', '/');

    private static string ProcurarPastaDados(string inicio)
    {
        for (var dir = new DirectoryInfo(inicio); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "curriculo-base.tex"))) return dir.FullName;
        return inicio;
    }
}
