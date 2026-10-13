namespace AdaptadorCurriculo.Modelos;

/// <summary>
/// Configuração da IA enviada pela página (cada pessoa usa a própria chave).
/// Tipo: "openai" (qualquer API compatível com OpenAI) ou "claude-code" (assinatura do Claude via CLI).
/// </summary>
public sealed record ConfigIa(string? Tipo, string? BaseUrl, string? ApiKey, string? Modelo, int? MaxTokens);

public sealed record PedidoAdaptacao(string? Titulo, string? Descricao, List<string>? Imagens, ConfigIa? Ia);

public sealed record PedidoSalvar(string? Tex);

/// <param name="PreenchimentoPagina2">Até onde vai o texto da 2ª página, em % da altura útil (100 = encosta na margem de baixo).</param>
public sealed record InfoPdf(int? Paginas, int? LinhasPagina2, int? PreenchimentoPagina2, DateTime Gerado);

public sealed record Saida(string Pasta, string? Tex, string? Relatorio, string? Vaga, InfoPdf? Pdf);

public sealed record ItemHistorico(string Pasta, string Titulo, DateTime Modificado);

public sealed record ResultadoCompilacao(InfoPdf? Pdf, string? Erro)
{
    public bool Ok => Erro is null;
}
