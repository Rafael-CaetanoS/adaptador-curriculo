using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;

namespace AdaptadorCurriculo.Servicos;

/// <summary>
/// Usa a assinatura do Claude (Pro/Max) através do Claude Code CLI (`claude -p`), sem ferramentas:
/// o CLI funciona só como modelo de linguagem. Autentica com o token gerado por `claude setup-token`.
/// A conversa inteira é enviada a cada chamada (o CLI é chamado sem sessão), com os prints da vaga como imagens.
/// </summary>
public sealed class ClaudeCodeChatClient(string token, string? modelo) : IChatClient
{
    private static readonly TimeSpan Limite = TimeSpan.FromMinutes(10);

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var lista = messages.ToList();
        var sistema = string.Join("\n\n", lista.Where(m => m.Role == ChatRole.System).Select(m => m.Text));
        var entrada = MontarEntrada(lista.Where(m => m.Role != ChatRole.System).ToList());

        var info = new ProcessStartInfo("claude")
        {
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false), StandardOutputEncoding = Encoding.UTF8,
        };
        foreach (var a in new[] { "-p", "--input-format", "stream-json", "--output-format", "stream-json", "--verbose", "--tools", "", "--max-turns", "1" })
            info.ArgumentList.Add(a);
        if (!string.IsNullOrWhiteSpace(sistema)) { info.ArgumentList.Add("--system-prompt"); info.ArgumentList.Add(sistema); }
        if (!string.IsNullOrWhiteSpace(modelo)) { info.ArgumentList.Add("--model"); info.ArgumentList.Add(modelo); }
        info.Environment["CLAUDE_CODE_OAUTH_TOKEN"] = token;
        info.Environment.Remove("ANTHROPIC_API_KEY"); // garante que usa a assinatura

        Process proc;
        try { proc = Process.Start(info)!; }
        catch (System.ComponentModel.Win32Exception)
        {
            throw new InvalidOperationException("O Claude Code CLI não está disponível neste servidor (rode pelo Docker).");
        }

        using (proc)
        using (var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            limite.CancelAfter(Limite);
            try
            {
                await proc.StandardInput.WriteLineAsync(entrada.AsMemory(), limite.Token);
                proc.StandardInput.Close();
                var stdout = proc.StandardOutput.ReadToEndAsync(limite.Token);
                var stderr = proc.StandardError.ReadToEndAsync(limite.Token);
                await proc.WaitForExitAsync(limite.Token);
                return Interpretar(await stdout, await stderr);
            }
            catch (OperationCanceledException)
            {
                proc.Kill(entireProcessTree: true);
                if (cancellationToken.IsCancellationRequested) throw;
                throw new InvalidOperationException($"O Claude Code demorou mais de {Limite.TotalMinutes} minutos e foi interrompido.");
            }
        }
    }

    /// <summary>Uma única mensagem de usuário (formato stream-json do CLI) com o histórico e as imagens.</summary>
    private static string MontarEntrada(List<ChatMessage> conversa)
    {
        var blocos = new JsonArray();
        foreach (var m in conversa)
        {
            var rotulo = m.Role == ChatRole.Assistant ? "SUA RESPOSTA ANTERIOR" : "PEDIDO";
            blocos.Add(new JsonObject { ["type"] = "text", ["text"] = conversa.Count > 1 ? $"=== {rotulo} ===\n{m.Text}" : m.Text });
            foreach (var img in m.Contents.OfType<DataContent>().Where(d => d.HasTopLevelMediaType("image")))
                blocos.Add(new JsonObject
                {
                    ["type"] = "image",
                    ["source"] = new JsonObject { ["type"] = "base64", ["media_type"] = img.MediaType, ["data"] = Convert.ToBase64String(img.Data.Span) },
                });
        }
        var msg = new JsonObject { ["type"] = "user", ["message"] = new JsonObject { ["role"] = "user", ["content"] = blocos } };
        return msg.ToJsonString();
    }

    private static ChatResponse Interpretar(string stdout, string stderr)
    {
        JsonNode? resultado = null;
        foreach (var linha in stdout.Split('\n'))
        {
            if (!linha.TrimStart().StartsWith('{')) continue;
            try { if (JsonNode.Parse(linha) is { } n && (string?)n["type"] == "result") resultado = n; }
            catch (JsonException) { }
        }

        if (resultado is null)
            throw new InvalidOperationException("O Claude Code não respondeu. " + Ultimas(stderr.Length > 0 ? stderr : stdout));

        var texto = (string?)resultado["result"] ?? "";
        if (resultado["is_error"]?.GetValue<bool>() == true)
        {
            if (texto.Contains("logged in", StringComparison.OrdinalIgnoreCase) || texto.Contains("401") || texto.Contains("token", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("A assinatura do Claude recusou o token. Gere outro com `claude setup-token` e atualize CLAUDE_CODE_OAUTH_TOKEN.");
            throw new InvalidOperationException("Erro do Claude Code: " + (texto.Length > 0 ? texto : Ultimas(stderr)));
        }

        var fim = (string?)resultado["subtype"] == "error_max_turns" ? ChatFinishReason.Length : ChatFinishReason.Stop;
        return new ChatResponse(new ChatMessage(ChatRole.Assistant, texto)) { FinishReason = fim };
    }

    private static string Ultimas(string s) => string.Join(' ', s.Split('\n', StringSplitOptions.RemoveEmptyEntries).TakeLast(3));

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var r = await GetResponseAsync(messages, options, cancellationToken);
        foreach (var u in r.ToChatResponseUpdates()) yield return u;
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }
}
