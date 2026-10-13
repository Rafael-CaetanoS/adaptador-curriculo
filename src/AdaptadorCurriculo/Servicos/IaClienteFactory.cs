using System.ClientModel;
using System.ClientModel.Primitives;
using AdaptadorCurriculo.Modelos;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AdaptadorCurriculo.Servicos;

/// <summary>
/// Cria o cliente de IA a partir da configuração de quem está usando.
/// Usa o formato da API do OpenAI (Chat Completions), aceito por OpenAI, Anthropic (Claude), Google Gemini,
/// Groq, OpenRouter, DeepSeek, Mistral, Ollama, LM Studio e outros — basta trocar URL base, chave e modelo.
/// Se a página não mandar nada, usa IA_BASE_URL / IA_API_KEY / IA_MODELO do servidor (.env).
/// </summary>
public sealed class IaClienteFactory(IConfiguration config)
{
    public const int MaxTokensPadrao = 16000;

    public sealed record ConfigResolvida(string BaseUrl, string ApiKey, string Modelo, int MaxTokens);

    public ConfigResolvida Resolver(ConfigIa? pedido)
    {
        var baseUrl = Preenchido(pedido?.BaseUrl) ?? config["IA_BASE_URL"];
        var modelo = Preenchido(pedido?.Modelo) ?? config["IA_MODELO"];
        var chave = Preenchido(pedido?.ApiKey) ?? config["IA_API_KEY"];
        var maxTokens = pedido?.MaxTokens ?? (int.TryParse(config["IA_MAX_TOKENS"], out var m) ? m : MaxTokensPadrao);

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(modelo))
            throw new IaNaoConfiguradaException("Configure a IA (URL base e modelo) no painel \"Configurar IA\" ou no .env do servidor.");
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new IaNaoConfiguradaException($"URL base inválida: {baseUrl}");

        // Ollama / LM Studio locais não exigem chave, mas o SDK precisa de algum valor
        return new ConfigResolvida(baseUrl.TrimEnd('/'), string.IsNullOrWhiteSpace(chave) ? "sem-chave" : chave, modelo, maxTokens);
    }

    public bool ServidorConfigurado => !string.IsNullOrWhiteSpace(config["IA_BASE_URL"]) && !string.IsNullOrWhiteSpace(config["IA_MODELO"]);
    public string? ModeloServidor => config["IA_MODELO"];
    public string? BaseUrlServidor => config["IA_BASE_URL"];

    public IChatClient Criar(ConfigResolvida c)
    {
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
