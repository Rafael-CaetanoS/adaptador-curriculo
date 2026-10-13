using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using AdaptadorCurriculo.Configuracao;
using AdaptadorCurriculo.Modelos;
using AdaptadorCurriculo.Servicos;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 60_000_000); // prints da vaga
builder.Services.AddSingleton<Caminhos>();
builder.Services.AddSingleton<LatexService>();
builder.Services.AddSingleton<SaidasService>();
builder.Services.AddSingleton<VerificadorTecnologias>();
builder.Services.AddSingleton<IaClienteFactory>();
builder.Services.AddSingleton<OrquestradorAdaptacao>();
builder.Services.AddSingleton<JobsService>();

var app = builder.Build();

// Senha opcional (APP_PASSWORD) via Basic Auth — obrigatória se expor em um servidor
var senha = app.Configuration["APP_PASSWORD"];
if (!string.IsNullOrEmpty(senha))
{
    app.Use(async (ctx, next) =>
    {
        if (SenhaConfere(ctx.Request.Headers.Authorization.ToString(), senha)) { await next(); return; }
        ctx.Response.Headers.WWWAuthenticate = "Basic realm=\"Adaptador de Curriculo\"";
        ctx.Response.StatusCode = 401;
    });
}

app.UseDefaultFiles();
app.UseStaticFiles();

var api = app.MapGroup("/api");

api.MapPost("/adaptar", (PedidoAdaptacao pedido, JobsService jobs, OrquestradorAdaptacao orquestrador, IaClienteFactory ia) =>
{
    if (string.IsNullOrWhiteSpace(pedido.Descricao) && (pedido.Imagens?.Count ?? 0) == 0)
        return Results.BadRequest(new { erro = "Cole a descrição da vaga ou envie um print dela." });
    if (pedido.Imagens?.Count > SaidasService.MaxImagens)
        return Results.BadRequest(new { erro = $"Envie no máximo {SaidasService.MaxImagens} imagens." });

    IaClienteFactory.ConfigResolvida config;
    try { config = ia.Resolver(pedido.Ia); }
    catch (IaNaoConfiguradaException e) { return Results.BadRequest(new { erro = e.Message }); }

    var job = jobs.Iniciar(j => orquestrador.ExecutarAsync(j, pedido, config));
    return job is null
        ? Results.Conflict(new { erro = "Já existe uma adaptação em andamento.", id = jobs.Ativo?.Id })
        : Results.Ok(new { id = job.Id });
});

api.MapGet("/status/{id}", (string id, JobsService jobs) =>
    jobs.Obter(id) is { } job ? Results.Ok(job.Retrato()) : Results.NotFound(new { erro = "Job não encontrado." }));

api.MapGet("/historico", (SaidasService saidas) => saidas.Listar());

api.MapGet("/saida", (string? pasta, Caminhos caminhos, SaidasService saidas) =>
    caminhos.PastaDeSaida(pasta) is { } abs ? Results.Ok(saidas.Ler(abs)) : Results.NotFound(new { erro = "Pasta não encontrada." }));

api.MapPost("/compilar", async (string? pasta, Caminhos caminhos, SaidasService saidas, LatexService latex, CancellationToken ct) =>
{
    if (caminhos.PastaDeSaida(pasta) is not { } abs) return Results.NotFound(new { erro = "Pasta não encontrada." });
    var r = await latex.CompilarAsync(abs, ct);
    return r.Ok ? Results.Ok(saidas.Ler(abs)) : Results.Json(new { erro = r.Erro }, statusCode: 500);
});

// Salva o LaTeX editado no editor e recompila
api.MapPost("/salvar", async (string? pasta, PedidoSalvar corpo, Caminhos caminhos, SaidasService saidas, LatexService latex, CancellationToken ct) =>
{
    if (caminhos.PastaDeSaida(pasta) is not { } abs) return Results.NotFound(new { erro = "Pasta não encontrada." });
    if (corpo.Tex is null || !corpo.Tex.Contains(@"\begin{document}")) return Results.BadRequest(new { erro = "LaTeX inválido." });
    await File.WriteAllTextAsync(Path.Combine(abs, "curriculo.tex"), corpo.Tex, ct);
    var r = await latex.CompilarAsync(abs, ct);
    var s = saidas.Ler(abs);
    return Results.Ok(new { s.Pasta, s.Tex, s.Relatorio, s.Vaga, s.Pdf, erroCompilacao = r.Erro });
});

api.MapGet("/pdf", (string? pasta, bool? baixar, Caminhos caminhos) =>
{
    var abs = caminhos.PastaDeSaida(pasta);
    var arquivo = abs is null ? null : Path.Combine(abs, "curriculo.pdf");
    return arquivo is not null && File.Exists(arquivo)
        ? EnviarPdf(arquivo, $"curriculo-{Path.GetFileName(abs)}.pdf", baixar == true)
        : Results.NotFound(new { erro = "PDF não encontrado." });
});

api.MapGet("/base", (Caminhos caminhos, SaidasService saidas) =>
    Results.Ok(new { tex = File.ReadAllText(caminhos.CurriculoBase), paginas = saidas.PaginasBase() }));

api.MapGet("/base-pdf", async (bool? baixar, SaidasService saidas, CancellationToken ct) =>
{
    var (arquivo, erro) = await saidas.PdfBaseAsync(ct);
    return arquivo is null ? Results.Json(new { erro }, statusCode: 500) : EnviarPdf(arquivo, "curriculo-base.pdf", baixar == true);
});

// IA configurada no servidor (.env), sem expor a chave
api.MapGet("/ia/padrao", (IaClienteFactory ia) =>
    Results.Ok(new { configurado = ia.ServidorConfigurado, baseUrl = ia.BaseUrlServidor, modelo = ia.ModeloServidor }));

// Testa a conexão com a IA escolhida na página
api.MapPost("/ia/testar", async (ConfigIa? config, IaClienteFactory ia, CancellationToken ct) =>
{
    try
    {
        var c = ia.Resolver(config);
        using var cliente = ia.Criar(c);
        var relogio = Stopwatch.StartNew();
        var r = await cliente.GetResponseAsync("Responda apenas: ok", new ChatOptions { MaxOutputTokens = 50 }, ct);
        return Results.Ok(new { ok = true, modelo = c.Modelo, resposta = r.Text.Trim(), ms = relogio.ElapsedMilliseconds });
    }
    catch (Exception e)
    {
        return Results.Ok(new { ok = false, erro = IaClienteFactory.Explicar(e) });
    }
});

app.Run();

static IResult EnviarPdf(string arquivo, string nome, bool baixar) =>
    Results.File(File.ReadAllBytes(arquivo), "application/pdf", baixar ? nome : null);

static bool SenhaConfere(string cabecalho, string senha)
{
    if (!cabecalho.StartsWith("Basic ", StringComparison.Ordinal)) return false;
    try
    {
        var credenciais = Encoding.UTF8.GetString(Convert.FromBase64String(cabecalho[6..]));
        var informada = credenciais[(credenciais.IndexOf(':') + 1)..];
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(informada), Encoding.UTF8.GetBytes(senha));
    }
    catch (FormatException) { return false; }
}
