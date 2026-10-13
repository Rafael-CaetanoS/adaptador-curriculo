using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using AdaptadorCurriculo.Modelos;

namespace AdaptadorCurriculo.Servicos;

/// <summary>Compila curriculo.tex com pdflatex (o mesmo do Overleaf) e mede o resultado.</summary>
public sealed partial class LatexService(ILogger<LatexService> logger)
{
    private static readonly TimeSpan Limite = TimeSpan.FromSeconds(90);

    public async Task<ResultadoCompilacao> CompilarAsync(string pasta, CancellationToken ct = default)
    {
        if (!File.Exists(Path.Combine(pasta, "curriculo.tex")))
            return new(null, "curriculo.tex não encontrado.");

        var (codigo, saida) = await ExecutarAsync("pdflatex", ["-interaction=nonstopmode", "-halt-on-error", "curriculo.tex"], pasta, ct);
        foreach (var ext in new[] { "aux", "out", "log" })
            File.Delete(Path.Combine(pasta, $"curriculo.{ext}"));

        if (codigo == -1) return new(null, "pdflatex não está disponível (rode pelo Docker).");
        if (codigo != 0)
        {
            var linhas = saida.Split('\n').Where(l => l.StartsWith('!') || LinhaErro().IsMatch(l)).Take(6);
            var erro = string.Join('\n', linhas);
            return new(null, $"Erro ao compilar o LaTeX:\n{(erro.Length > 0 ? erro : saida[^Math.Min(800, saida.Length)..])}");
        }

        var m = PaginasEscritas().Match(saida);
        int? paginas = m.Success ? int.Parse(m.Groups[1].Value) : null;
        int? linhasPagina2 = paginas >= 2 ? await ContarLinhasAsync(pasta, 2, ct) : null;
        int? preenchimento = paginas >= 2 ? await PreenchimentoAsync(pasta, 2, ct) : null;
        var info = new InfoPdf(paginas, linhasPagina2, preenchimento, DateTime.UtcNow);
        await File.WriteAllTextAsync(Path.Combine(pasta, "pdf.json"), JsonSerializer.Serialize(info, JsonSerializerOptions.Web), ct);
        return new(info, null);
    }

    public static InfoPdf? LerInfo(string pasta)
    {
        var json = Path.Combine(pasta, "pdf.json");
        if (!File.Exists(json) || !File.Exists(Path.Combine(pasta, "curriculo.pdf"))) return null;
        try { return JsonSerializer.Deserialize<InfoPdf>(File.ReadAllText(json), JsonSerializerOptions.Web); }
        catch (JsonException) { return null; }
    }

    /// <summary>Linhas com texto numa página (para saber se a 2ª página está bem preenchida).</summary>
    private async Task<int?> ContarLinhasAsync(string pasta, int pagina, CancellationToken ct)
    {
        var (codigo, texto) = await ExecutarAsync("pdftotext", ["-f", $"{pagina}", "-l", $"{pagina}", "-layout", "curriculo.pdf", "-"], pasta, ct);
        return codigo == 0 ? texto.Split('\n').Count(l => !string.IsNullOrWhiteSpace(l)) : null;
    }

    /// <summary>
    /// Até onde o texto vai na página, em % da altura útil (descontando a margem de baixo de 1,2 cm do layout).
    /// Mede o preenchimento de verdade: espaçamento maior também conta, ao contrário de contar linhas.
    /// </summary>
    private async Task<int?> PreenchimentoAsync(string pasta, int pagina, CancellationToken ct)
    {
        var (codigo, xml) = await ExecutarAsync("pdftotext", ["-f", $"{pagina}", "-l", $"{pagina}", "-bbox", "curriculo.pdf", "-"], pasta, ct);
        if (codigo != 0) return null;
        var altura = AlturaPagina().Match(xml);
        var fins = FimPalavra().Matches(xml).Select(m => double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture)).ToList();
        if (!altura.Success || fins.Count == 0) return null;
        const double MargemInferiorPt = 34; // 1,2 cm
        var util = double.Parse(altura.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) - MargemInferiorPt;
        return (int)Math.Round(Math.Min(100, fins.Max() / util * 100));
    }

    private async Task<(int Codigo, string Saida)> ExecutarAsync(string comando, string[] args, string pasta, CancellationToken ct)
    {
        var info = new ProcessStartInfo(comando) { WorkingDirectory = pasta, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var a in args) info.ArgumentList.Add(a);
        Process proc;
        try { proc = Process.Start(info)!; }
        catch (System.ComponentModel.Win32Exception) { return (-1, $"{comando} não encontrado"); }

        using (proc)
        using (var limite = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            limite.CancelAfter(Limite);
            try
            {
                var stdout = proc.StandardOutput.ReadToEndAsync(limite.Token);
                var stderr = proc.StandardError.ReadToEndAsync(limite.Token);
                await proc.WaitForExitAsync(limite.Token);
                return (proc.ExitCode, await stdout + await stderr);
            }
            catch (OperationCanceledException)
            {
                proc.Kill(entireProcessTree: true);
                if (ct.IsCancellationRequested) throw;
                logger.LogWarning("{Comando} passou de {Limite}s em {Pasta}", comando, Limite.TotalSeconds, pasta);
                return (1, $"{comando} demorou demais (mais de {Limite.TotalSeconds}s).");
            }
        }
    }

    [GeneratedRegex(@"^l\.\d+")]
    private static partial Regex LinhaErro();

    [GeneratedRegex(@"Output written on .*?\((\d+) pages?")]
    private static partial Regex PaginasEscritas();

    [GeneratedRegex(@"<page [^>]*height=""([\d.]+)""")]
    private static partial Regex AlturaPagina();

    [GeneratedRegex(@"yMax=""([\d.]+)""")]
    private static partial Regex FimPalavra();
}
