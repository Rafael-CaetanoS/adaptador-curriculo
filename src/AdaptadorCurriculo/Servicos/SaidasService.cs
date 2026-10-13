using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AdaptadorCurriculo.Configuracao;
using AdaptadorCurriculo.Modelos;

namespace AdaptadorCurriculo.Servicos;

/// <summary>Pastas de saída (uma por vaga), prints enviados e o PDF do currículo base.</summary>
public sealed partial class SaidasService(Caminhos caminhos, LatexService latex)
{
    private readonly SemaphoreSlim _compilandoBase = new(1, 1);

    public IReadOnlyList<ItemHistorico> Listar() =>
        Directory.EnumerateDirectories(caminhos.Saidas)
            .Select(dir => new ItemHistorico(caminhos.Relativo(dir), TituloDoRelatorio(dir), Directory.GetLastWriteTimeUtc(dir)))
            .OrderByDescending(i => i.Modificado)
            .ToList();

    public Saida Ler(string pasta)
    {
        string? Ler(string arquivo) => File.Exists(Path.Combine(pasta, arquivo)) ? File.ReadAllText(Path.Combine(pasta, arquivo)) : null;
        return new Saida(caminhos.Relativo(pasta), Ler("curriculo.tex"), Ler("relatorio.md"), Ler("vaga.md"), LatexService.LerInfo(pasta));
    }

    /// <summary>Cria saidas/AAAA-MM-DD-empresa-cargo (com -2, -3… se já existir).</summary>
    public string CriarPasta(string? empresa, string? cargo)
    {
        var nome = $"{DateTime.Now:yyyy-MM-dd}-{Slug(string.IsNullOrWhiteSpace(empresa) ? "vaga" : empresa)}-{Slug(cargo ?? "curriculo")}".Trim('-');
        var pasta = Path.Combine(caminhos.Saidas, nome);
        for (var n = 2; Directory.Exists(pasta); n++) pasta = Path.Combine(caminhos.Saidas, $"{nome}-{n}");
        Directory.CreateDirectory(pasta);
        return pasta;
    }

    private static readonly Dictionary<string, string> TiposImagem = new()
    {
        ["image/png"] = "png", ["image/jpeg"] = "jpg", ["image/webp"] = "webp", ["image/gif"] = "gif",
    };

    public const int MaxImagens = 8;

    /// <summary>Decodifica os prints (data URLs), salva em entradas/&lt;id&gt;/ e devolve (bytes, tipo).</summary>
    public List<(byte[] Bytes, string Tipo)> SalvarImagens(string id, IReadOnlyList<string>? dataUrls)
    {
        var imagens = new List<(byte[], string)>();
        if (dataUrls is null || dataUrls.Count == 0) return imagens;
        if (dataUrls.Count > MaxImagens) throw new ArgumentException($"Envie no máximo {MaxImagens} imagens.");
        var dir = Path.Combine(caminhos.Entradas, id);
        Directory.CreateDirectory(dir);
        for (var i = 0; i < dataUrls.Count; i++)
        {
            var m = DataUrl().Match(dataUrls[i]);
            if (!m.Success || !TiposImagem.TryGetValue(m.Groups[1].Value, out var ext))
                throw new ArgumentException("Formato de imagem não suportado (use PNG, JPG ou WEBP).");
            var bytes = Convert.FromBase64String(m.Groups[2].Value);
            File.WriteAllBytes(Path.Combine(dir, $"vaga-{i + 1}.{ext}"), bytes);
            imagens.Add((bytes, m.Groups[1].Value));
        }
        return imagens;
    }

    /// <summary>PDF do currículo base, recompilado só quando o curriculo-base.tex muda.</summary>
    public async Task<(string? Arquivo, string? Erro)> PdfBaseAsync(CancellationToken ct)
    {
        var pdf = Path.Combine(caminhos.PreviewBase, "curriculo.pdf");
        await _compilandoBase.WaitAsync(ct);
        try
        {
            if (File.Exists(pdf) && File.GetLastWriteTimeUtc(pdf) >= File.GetLastWriteTimeUtc(caminhos.CurriculoBase))
                return (pdf, null);
            Directory.CreateDirectory(caminhos.PreviewBase);
            File.Copy(caminhos.CurriculoBase, Path.Combine(caminhos.PreviewBase, "curriculo.tex"), overwrite: true);
            var r = await latex.CompilarAsync(caminhos.PreviewBase, ct);
            return r.Ok ? (pdf, null) : (null, r.Erro);
        }
        finally { _compilandoBase.Release(); }
    }

    public int? PaginasBase() => LatexService.LerInfo(caminhos.PreviewBase)?.Paginas;

    private static string TituloDoRelatorio(string dir)
    {
        var relatorio = Path.Combine(dir, "relatorio.md");
        if (!File.Exists(relatorio)) return Path.GetFileName(dir);
        var linha = File.ReadLines(relatorio).FirstOrDefault(l => l.StartsWith("# "));
        return linha is null ? Path.GetFileName(dir) : TituloRelatorio().Replace(linha, "");
    }

    public static string Slug(string texto)
    {
        var semAcento = new StringBuilder();
        foreach (var c in texto.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) semAcento.Append(c);
        var slug = NaoAlfanumerico().Replace(semAcento.ToString().ToLowerInvariant(), "-").Trim('-');
        return slug.Length > 50 ? slug[..50].TrimEnd('-') : slug;
    }

    [GeneratedRegex(@"^data:(image/[a-z]+);base64,(.+)$", RegexOptions.Singleline)]
    private static partial Regex DataUrl();

    [GeneratedRegex(@"^#\s*(Relatório\s*[—-]\s*)?")]
    private static partial Regex TituloRelatorio();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NaoAlfanumerico();
}
