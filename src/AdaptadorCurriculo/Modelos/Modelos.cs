namespace AdaptadorCurriculo.Modelos;

/// <summary>Configuração da IA enviada pela página (cada pessoa usa a própria chave).</summary>
public sealed record ConfigIa(string? BaseUrl, string? ApiKey, string? Modelo, int? MaxTokens);

public sealed record PedidoAdaptacao(string? Titulo, string? Descricao, List<string>? Imagens, ConfigIa? Ia);

public sealed record PedidoSalvar(string? Tex);

public sealed record InfoPdf(int? Paginas, int? LinhasPagina2, DateTime Gerado);

public sealed record Saida(string Pasta, string? Tex, string? Relatorio, string? Vaga, InfoPdf? Pdf);

public sealed record ItemHistorico(string Pasta, string Titulo, DateTime Modificado);

public sealed record ResultadoCompilacao(InfoPdf? Pdf, string? Erro)
{
    public bool Ok => Erro is null;
}
