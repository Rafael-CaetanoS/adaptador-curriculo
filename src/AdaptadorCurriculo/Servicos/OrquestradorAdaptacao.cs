using System.Text;
using System.Text.RegularExpressions;
using AdaptadorCurriculo.Configuracao;
using AdaptadorCurriculo.Modelos;
using Microsoft.Extensions.AI;

namespace AdaptadorCurriculo.Servicos;

/// <summary>
/// Faz a adaptação de ponta a ponta com qualquer IA:
/// monta o prompt → IA devolve currículo + relatório → compila → confere 2 páginas e a trava
/// "não inventar" → se precisar, devolve os problemas para a IA corrigir (até 3 rodadas).
/// </summary>
public sealed partial class OrquestradorAdaptacao(
    Caminhos caminhos,
    LatexService latex,
    SaidasService saidas,
    VerificadorTecnologias verificador,
    IaClienteFactory iaFactory,
    ILogger<OrquestradorAdaptacao> logger)
{
    private const int MaxCorrecoes = 3;
    private const int MinPreenchimentoPagina2 = 85; // % da altura útil da 2ª página

    public async Task ExecutarAsync(Job job, PedidoAdaptacao pedido, IaClienteFactory.ConfigResolvida ia, CancellationToken ct = default)
    {
        try
        {
            var imagens = saidas.SalvarImagens(job.Id, pedido.Imagens);
            job.Registrar("Lendo currículo base e perfil extra");
            var curriculoBase = await File.ReadAllTextAsync(caminhos.CurriculoBase, ct);
            var perfil = File.Exists(caminhos.PerfilExtra) ? await File.ReadAllTextAsync(caminhos.PerfilExtra, ct) : "";
            var fonte = VerificadorTecnologias.MontarFonte(curriculoBase, perfil);

            var mensagens = new List<ChatMessage>
            {
                new(ChatRole.System, await File.ReadAllTextAsync(caminhos.Prompt("sistema.md"), ct)),
                await MontarPedidoAsync(pedido, curriculoBase, perfil, imagens, ct),
            };

            using var cliente = iaFactory.Criar(ia);
            var opcoes = new ChatOptions { MaxOutputTokens = ia.MaxTokens };

            job.Registrar(imagens.Count > 0 ? $"Lendo os prints e analisando a vaga com {ia.Nome}" : $"Analisando a vaga com {ia.Nome}");
            var resposta = await PerguntarAsync(cliente, mensagens, opcoes, ct);
            var blocos = ExtrairBlocos(resposta);
            if (!blocos.TryGetValue("curriculo_tex", out var tex) || !tex.Contains(@"\end{document}"))
                throw new InvalidOperationException(RespostaIncompleta(resposta));

            var pasta = saidas.CriarPasta(blocos.GetValueOrDefault("empresa"), blocos.GetValueOrDefault("titulo_vaga") ?? pedido.Titulo);
            job.Registrar($"Salvando em {caminhos.Relativo(pasta)}");
            await SalvarAsync(pasta, "vaga.md", blocos.GetValueOrDefault("vaga_md") ?? $"# {pedido.Titulo}\n\n{pedido.Descricao}", ct);
            await SalvarAsync(pasta, "relatorio.md", blocos.GetValueOrDefault("relatorio_md") ?? "# Relatório\n\n_A IA não devolveu relatório._", ct);
            await SalvarAsync(pasta, "curriculo.tex", tex, ct);

            ResultadoCompilacao compilacao;
            IReadOnlyList<string> semBase;
            for (var rodada = 0; ; rodada++)
            {
                job.Registrar(rodada == 0 ? "Compilando o PDF para conferir as 2 páginas" : $"Compilando a versão corrigida ({rodada}/{MaxCorrecoes})");
                compilacao = await latex.CompilarAsync(pasta, ct);
                semBase = verificador.SemBase(tex, fonte);
                var problemas = Problemas(compilacao, semBase);
                if (problemas.Count == 0 || rodada == MaxCorrecoes) break;

                job.Registrar("Pedindo ajuste à IA: " + string.Join("; ", problemas.Select(p => p.Resumo)));
                mensagens.Add(new ChatMessage(ChatRole.Assistant, resposta));
                mensagens.Add(new ChatMessage(ChatRole.User, PedidoDeCorrecao(problemas)));
                resposta = await PerguntarAsync(cliente, mensagens, opcoes, ct);
                var corrigido = ExtrairBlocos(resposta).GetValueOrDefault("curriculo_tex");
                if (corrigido is null || !corrigido.Contains(@"\end{document}"))
                {
                    job.Registrar("A IA não devolveu um currículo completo na correção; mantendo a versão anterior");
                    break;
                }
                tex = corrigido;
                await SalvarAsync(pasta, "curriculo.tex", tex, ct);
            }

            await AnexarConferenciaAsync(pasta, compilacao, semBase, ct);
            job.AvisoPdf = compilacao.Erro;
            job.Resposta = blocos.GetValueOrDefault("resumo_md");
            job.Saida = saidas.Ler(pasta);
            job.Status = "done";
            job.Registrar("Pronto");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Falha na adaptação {Job}", job.Id);
            job.Erro = IaClienteFactory.Explicar(e);
            job.Status = "error";
            job.Registrar("Falhou");
        }
    }

    private async Task<ChatMessage> MontarPedidoAsync(PedidoAdaptacao pedido, string curriculoBase, string perfil,
        List<(byte[] Bytes, string Tipo)> imagens, CancellationToken ct)
    {
        var texto = new StringBuilder()
            .AppendLine("# CURRÍCULO BASE (LaTeX)").AppendLine(curriculoBase).AppendLine()
            .AppendLine("# PERFIL EXTRA (coisas reais que não estão no currículo; comentários <!-- --> são só exemplos e NÃO valem)")
            .AppendLine(string.IsNullOrWhiteSpace(perfil) ? "(vazio)" : perfil).AppendLine()
            .AppendLine("# LAYOUT DE REFERÊNCIA (siga a estrutura; o conteúdo é de outra vaga)")
            .AppendLine(await File.ReadAllTextAsync(caminhos.Prompt("layout-referencia.tex"), ct)).AppendLine()
            .AppendLine("# VAGA")
            .AppendLine($"Título informado: {(string.IsNullOrWhiteSpace(pedido.Titulo) ? "(não informado — extraia da vaga)" : pedido.Titulo)}")
            .AppendLine()
            .AppendLine(string.IsNullOrWhiteSpace(pedido.Descricao) ? "(descrição nas imagens anexas)" : pedido.Descricao);
        if (imagens.Count > 0) texto.AppendLine().AppendLine($"Há {imagens.Count} imagem(ns) da vaga anexada(s): transcreva e use todas.");

        var conteudo = new List<AIContent> { new TextContent(texto.ToString()) };
        conteudo.AddRange(imagens.Select(i => new DataContent(i.Bytes, i.Tipo)));
        return new ChatMessage(ChatRole.User, conteudo);
    }

    private static async Task<string> PerguntarAsync(IChatClient cliente, List<ChatMessage> mensagens, ChatOptions opcoes, CancellationToken ct)
    {
        var r = await cliente.GetResponseAsync(mensagens, opcoes, ct);
        if (r.FinishReason == ChatFinishReason.Length)
            throw new InvalidOperationException("A resposta da IA foi cortada por limite de tamanho. Aumente o \"Máx. tokens\" no painel \"Configurar IA\".");
        return r.Text;
    }

    private sealed record Problema(string Resumo, string Detalhe);

    private static List<Problema> Problemas(ResultadoCompilacao c, IReadOnlyList<string> semBase)
    {
        var lista = new List<Problema>();
        if (!c.Ok)
            lista.Add(new("erro de LaTeX", $"O LaTeX não compilou. Corrija o erro:\n{c.Erro}"));
        else if (c.Pdf?.Paginas > 2)
            lista.Add(new($"{c.Pdf.Paginas} páginas", $"O PDF ficou com {c.Pdf.Paginas} páginas. Precisa de exatamente 2: corte seguindo a ordem das regras ou reduza os espaçamentos."));
        else if (c.Pdf?.Paginas < 2)
            lista.Add(new("1 página", "O PDF ficou com 1 página. Precisa de 2 páginas cheias: devolva conteúdo relevante do currículo base e aumente os espaçamentos."));
        else if (c.Pdf?.PreenchimentoPagina2 < MinPreenchimentoPagina2)
            lista.Add(new($"2ª página {c.Pdf.PreenchimentoPagina2}% preenchida",
                $"O texto da 2ª página só vai até {c.Pdf.PreenchimentoPagina2}% da altura (mínimo {MinPreenchimentoPagina2}%). Preencha mais: devolva bullets/detalhes relevantes do currículo base ou aumente \\titlespacing e os \\vspace, sem passar de 2 páginas."));
        if (semBase.Count > 0)
            lista.Add(new($"sem base: {string.Join(", ", semBase)}",
                $"Estes termos aparecem no currículo, mas não existem no currículo base nem no perfil extra: {string.Join(", ", semBase)}. Remova-os do currículo (eles podem ir para \"O que falta\" no relatório)."));
        return lista;
    }

    private static string PedidoDeCorrecao(List<Problema> problemas) =>
        "O sistema compilou e conferiu o currículo e encontrou problemas:\n\n" +
        string.Join("\n\n", problemas.Select((p, i) => $"{i + 1}. {p.Detalhe}")) +
        "\n\nDevolva SOMENTE o bloco <curriculo_tex>…</curriculo_tex> com o documento LaTeX completo corrigido.";

    private static async Task AnexarConferenciaAsync(string pasta, ResultadoCompilacao c, IReadOnlyList<string> semBase, CancellationToken ct)
    {
        var sb = new StringBuilder("\n\n## Conferência automática\n\n");
        sb.AppendLine(c.Ok
            ? $"- PDF compilado: **{c.Pdf?.Paginas} página(s)**, 2ª página {c.Pdf?.PreenchimentoPagina2?.ToString() ?? "?"}% preenchida."
            : $"- ⚠️ O LaTeX não compilou: {c.Erro?.Split('\n').Skip(1).FirstOrDefault()}");
        sb.AppendLine(semBase.Count == 0
            ? "- Trava \"não inventar\": nenhuma tecnologia sem base no currículo/perfil."
            : $"- ⚠️ Trava \"não inventar\": a IA manteve termos sem base — **revise antes de enviar**: {string.Join(", ", semBase)}.");
        await File.AppendAllTextAsync(Path.Combine(pasta, "relatorio.md"), sb.ToString(), ct);
    }

    private static Task SalvarAsync(string pasta, string arquivo, string conteudo, CancellationToken ct) =>
        File.WriteAllTextAsync(Path.Combine(pasta, arquivo), conteudo.Trim() + "\n", ct);

    /// <summary>Lê os blocos &lt;nome&gt;…&lt;/nome&gt; da resposta (tolerando cercas ``` em volta).</summary>
    public static Dictionary<string, string> ExtrairBlocos(string resposta)
    {
        var blocos = new Dictionary<string, string>();
        foreach (Match m in Bloco().Matches(resposta))
            blocos[m.Groups[1].Value] = Cerca().Replace(m.Groups[2].Value.Trim(), "").Trim();
        return blocos;
    }

    private static string RespostaIncompleta(string resposta) =>
        resposta.Contains("<curriculo_tex>") && !resposta.Contains("</curriculo_tex>")
            ? "A resposta da IA veio cortada no meio do currículo. Aumente o \"Máx. tokens\" no painel \"Configurar IA\"."
            : "A IA não devolveu o currículo no formato esperado. Tente de novo ou use um modelo mais capaz.";

    [GeneratedRegex(@"<(titulo_vaga|empresa|vaga_md|curriculo_tex|relatorio_md|resumo_md)>(.*?)</\1>", RegexOptions.Singleline)]
    private static partial Regex Bloco();

    [GeneratedRegex(@"^```[a-zA-Z]*\s*|\s*```$")]
    private static partial Regex Cerca();
}
