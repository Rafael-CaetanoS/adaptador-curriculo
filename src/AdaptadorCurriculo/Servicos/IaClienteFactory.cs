using System.ClientModel;
using System.ClientModel.Primitives;
using AdaptadorCurriculo.Modelos;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AdaptadorCurriculo.Servicos;

/// <summary>
/// Cria o cliente de IA a partir da configuração de quem está usando. Dois tipos:
/// - "openai": formato da API do OpenAI (Chat Completions), aceito por OpenAI, Anthropic (Claude), Google Gemini,
///   Groq, OpenRouter, DeepSeek, Mistral, Ollama, LM Studio e outros — basta trocar URL base, chave e modelo.
/// - "claude-code": assinatura do Claude (Pro/Max) pelo Claude Code CLI, com o token de `claude setup-token`.
/// Se a página não mandar nada, usa o .env do servidor (IA_BASE_URL/IA_MODELO/IA_API_KEY ou CLAUDE_CODE_OAUTH_TOKEN).
/// </summary>
public sealed class IaClienteFactory(IConfiguration config)
{
    public const int MaxTokensPadrao = 16000;
    public const string TipoOpenAi = "openai";
    public const string TipoClaudeCode = "claude-code";

    public sealed record ConfigResolvida(string Tipo, string BaseUrl, string ApiKey, string Modelo, int MaxTokens)
    {
        /// <summary>Nome para mostrar no progresso.</summary>
        public string Nome => Tipo != TipoClaudeCode ? Modelo : string.IsNullOrEmpty(Modelo) ? "assinatura Claude" : $"assinatura Claude ({Modelo})";
    }

    public ConfigResolvida Resolver(ConfigIa? pedido)
    {
        var maxTokens = pedido?.MaxTokens ?? (int.TryParse(config["IA_MAX_TOKENS"], out var m) ? m : MaxTokensPadrao);
        var pediuAlgo = pedido is not null && (Preenchido(pedido.Tipo) ?? Preenchido(pedido.BaseUrl) ?? Preenchido(pedido.Modelo)) is not null;
        var tipo = pediuAlgo ? Preenchido(pedido!.Tipo) ?? TipoOpenAi : TipoServidor;

        if (tipo == TipoClaudeCode)
        {
            var token = Preenchido(pedido?.ApiKey) ?? Preenchido(config["CLAUDE_CODE_OAUTH_TOKEN"]);
            if (token is null)
                throw new IaNaoConfiguradaException("Para usar a assinatura do Claude, gere um token com `claude setup-token` e coloque em CLAUDE_CODE_OAUTH_TOKEN no .env (ou cole no painel \"Configurar IA\").");
            return new ConfigResolvida(TipoClaudeCode, "", token, Preenchido(pedido?.Modelo) ?? config["CLAUDE_CODE_MODELO"] ?? "", maxTokens);
        }

        var baseUrl = Preenchido(pedido?.BaseUrl) ?? config["IA_BASE_URL"];
        var modelo = Preenchido(pedido?.Modelo) ?? config["IA_MODELO"];
        var chave = Preenchido(pedido?.ApiKey) ?? config["IA_API_KEY"];

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(modelo))
            throw new IaNaoConfiguradaException("Configure a IA (URL base e modelo) no painel \"Configurar IA\" ou no .env do servidor.");
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new IaNaoConfiguradaException($"URL base inválida: {baseUrl}");

        // Ollama / LM Studio locais não exigem chave, mas o SDK precisa de algum valor
        return new ConfigResolvida(TipoOpenAi, baseUrl.TrimEnd('/'), string.IsNullOrWhiteSpace(chave) ? "sem-chave" : chave, modelo, maxTokens);
    }

    private bool ServidorOpenAi => !string.IsNullOrWhiteSpace(config["IA_BASE_URL"]) && !string.IsNullOrWhiteSpace(config["IA_MODELO"]);
    public bool ServidorClaudeCode => !string.IsNullOrWhiteSpace(config["CLAUDE_CODE_OAUTH_TOKEN"]);

    /// <summary>IA padrão do servidor: IA_TIPO, ou a API se estiver configurada, senão a assinatura do Claude.</summary>
    public string TipoServidor => Preenchido(config["IA_TIPO"]) ?? (!ServidorOpenAi && ServidorClaudeCode ? TipoClaudeCode : TipoOpenAi);
    public bool ServidorConfigurado => TipoServidor == TipoClaudeCode ? ServidorClaudeCode : ServidorOpenAi;
    public string? ModeloServidor => TipoServidor == TipoClaudeCode ? config["CLAUDE_CODE_MODELO"] ?? "assinatura Claude" : config["IA_MODELO"];
    public string? BaseUrlServidor => TipoServidor == TipoClaudeCode ? null : config["IA_BASE_URL"];

    public IChatClient Criar(ConfigResolvida c)
    {
        if (c.Tipo == TipoClaudeCode) return new ClaudeCodeChatClient(c.ApiKey, c.Modelo);
        var opcoes = new OpenAIClientOptions
        {
            Endpoint = new Uri(c.BaseUrl + "/"),
            NetworkTimeout = TimeSpan.FromMinutes(10),
            RetryPolicy = new ClientRetryPolicy(maxRetries: 1),
        };
        return new OpenAIClient(new ApiKeyCredential(c.ApiKey), opcoes).GetChatClient(c.Modelo).AsIChatClient();
    }

    /// <summary>Traduz erros do provedor para mensagens úteis.</summary>
    public static string Explicar(Exception e) => e switch
    {
        AggregateException ag when ag.InnerExceptions.Count > 0 => Explicar(ag.InnerExceptions[^1]),
        IaNaoConfiguradaException => e.Message,
        HttpRequestException { InnerException: { } interno } => $"Não consegui conectar na IA: {interno.Message}",
        ClientResultException { Status: 401 or 403 } => "A IA recusou a chave (401/403). Confira a chave de API no painel \"Configurar IA\".",
        ClientResultException { Status: 404 } => "Modelo ou URL não encontrados (404). Confira o nome do modelo e a URL base.",
        ClientResultException { Status: 429 } => "Limite de uso da IA atingido (429). Espere um pouco ou confira os créditos da conta.",
        ClientResultException { Status: 0 } => $"Não consegui conectar na IA. Confira a URL base. ({e.Message})",
        ClientResultException c => $"Erro da IA ({c.Status}): {c.Message}",
        HttpRequestException or TaskCanceledException => $"Não consegui falar com a IA: {e.Message}",
        _ => e.Message,
    };

    private static string? Preenchido(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

public sealed class IaNaoConfiguradaException(string mensagem) : Exception(mensagem);
