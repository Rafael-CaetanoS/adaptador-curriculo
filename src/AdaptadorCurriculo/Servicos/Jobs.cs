using System.Collections.Concurrent;
using AdaptadorCurriculo.Modelos;

namespace AdaptadorCurriculo.Servicos;

/// <summary>Uma adaptação em andamento: a página consulta o progresso a cada 2s.</summary>
public sealed class Job
{
    private readonly List<string> _log = ["Iniciando…"];
    private readonly Lock _trava = new();

    public string Id { get; } = Guid.NewGuid().ToString();
    public DateTime Inicio { get; } = DateTime.UtcNow;
    public string Status { get; set; } = "running";
    public string? Resposta { get; set; }
    public string? Erro { get; set; }
    public string? AvisoPdf { get; set; }
    public Saida? Saida { get; set; }

    public void Registrar(string etapa) { lock (_trava) _log.Add(etapa); }

    public object Retrato()
    {
        lock (_trava)
            return new
            {
                Id, Status, Resposta, Erro, AvisoPdf, Saida,
                Log = _log.ToArray(),
                Segundos = (int)(DateTime.UtcNow - Inicio).TotalSeconds,
            };
    }
}

/// <summary>Guarda os jobs em memória e garante uma adaptação por vez.</summary>
public sealed class JobsService
{
    private readonly ConcurrentDictionary<string, Job> _jobs = new();
    private Job? _ativo;
    private readonly Lock _trava = new();

    public Job? Ativo { get { lock (_trava) return _ativo?.Status == "running" ? _ativo : null; } }

    public Job? Obter(string id) => _jobs.GetValueOrDefault(id);

    /// <summary>Cria e dispara o job; devolve null se já houver um rodando.</summary>
    public Job? Iniciar(Func<Job, Task> trabalho)
    {
        lock (_trava)
        {
            if (_ativo?.Status == "running") return null;
            var job = new Job();
            _jobs[job.Id] = job;
            _ativo = job;
            _ = Task.Run(() => trabalho(job));
            return job;
        }
    }
}
