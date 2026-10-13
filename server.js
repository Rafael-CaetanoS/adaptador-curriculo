// Adaptador de currículo — servidor web.
// Feito para rodar no Docker (ver README.md): o container já traz Node, Claude Code e LaTeX.
// Roda o comando /adaptar-curriculo do Claude Code em modo headless (claude -p)
// e compila o PDF com pdflatex (o mesmo compilador do Overleaf).

const http = require('http');
const fs = require('fs');
const path = require('path');
const { spawn } = require('child_process');
const crypto = require('crypto');

const PORT = Number(process.env.PORT) || 3210;
const HOST = process.env.HOST || '127.0.0.1';
const SENHA = process.env.APP_PASSWORD || '';
const ROOT = __dirname;
const SAIDAS = path.join(ROOT, 'saidas');
const WIN = process.platform === 'win32';

const jobs = new Map();
let jobAtivo = null;

function json(res, status, body) {
  res.writeHead(status, { 'Content-Type': 'application/json; charset=utf-8' });
  res.end(JSON.stringify(body));
}

function lerBody(req) {
  return new Promise((resolve, reject) => {
    let data = '';
    req.on('data', (c) => {
      data += c;
      if (data.length > 60_000_000) reject(new Error('Envio muito grande (imagens demais?)'));
    });
    req.on('end', () => resolve(data));
    req.on('error', reject);
  });
}

// Executa um comando e devolve { code, out }
function executar(cmd, args, opts = {}) {
  return new Promise((resolve) => {
    const argv = WIN ? args.map((a) => (/\s/.test(a) ? `"${a}"` : a)) : args;
    const p = spawn(cmd, argv, { shell: WIN, windowsHide: true, ...opts });
    let out = '';
    p.stdout?.on('data', (c) => { out += c; });
    p.stderr?.on('data', (c) => { out += c; });
    p.on('close', (code) => resolve({ code, out }));
    p.on('error', (e) => resolve({ code: -1, out: e.message }));
  });
}

// Garante que a pasta pedida está dentro de saidas/ (evita ../)
function pastaSegura(rel) {
  if (!rel) return null;
  const abs = path.resolve(ROOT, rel);
  if (!abs.startsWith(SAIDAS + path.sep)) return null;
  return fs.existsSync(abs) ? abs : null;
}

function lerSaida(abs) {
  const ler = (f) => {
    const p = path.join(abs, f);
    return fs.existsSync(p) ? fs.readFileSync(p, 'utf8') : null;
  };
  let pdf = null;
  try { pdf = JSON.parse(ler('pdf.json')); } catch {}
  return {
    pasta: path.relative(ROOT, abs).replace(/\\/g, '/'),
    tex: ler('curriculo.tex'),
    relatorio: ler('relatorio.md'),
    vaga: ler('vaga.md'),
    pdf: pdf && fs.existsSync(path.join(abs, 'curriculo.pdf')) ? pdf : null,
  };
}

// Compila curriculo.tex -> curriculo.pdf e grava o nº de páginas em pdf.json
async function compilarPdf(abs) {
  if (!fs.existsSync(path.join(abs, 'curriculo.tex'))) return { erro: 'curriculo.tex não encontrado.' };
  const { code, out } = await executar('pdflatex', ['-interaction=nonstopmode', '-halt-on-error', 'curriculo.tex'], { cwd: abs });
  for (const ext of ['aux', 'out', 'log']) fs.rmSync(path.join(abs, `curriculo.${ext}`), { force: true });
  if (code !== 0) {
    const erroTex = out.split('\n').filter((l) => l.startsWith('!') || /^l\.\d+/.test(l)).slice(0, 6).join('\n');
    return { erro: code === -1 ? 'pdflatex não está disponível (rode pelo Docker).' : `Erro ao compilar o LaTeX:\n${erroTex || out.slice(-800)}` };
  }
  const m = out.match(/Output written on .*?\((\d+) pages?/);
  const info = { paginas: m ? Number(m[1]) : null, gerado: new Date().toISOString() };
  fs.writeFileSync(path.join(abs, 'pdf.json'), JSON.stringify(info));
  return info;
}

// PDF do currículo base (versão salva no projeto), para comparar com as versões adaptadas.
// Compilado em .preview/base/ e reaproveitado enquanto curriculo-base.tex não mudar.
const BASE_TEX = path.join(ROOT, 'curriculo-base.tex');
const PREVIEW_BASE = path.join(ROOT, '.preview', 'base');
let compilandoBase = null;

function pdfBase() {
  const pdf = path.join(PREVIEW_BASE, 'curriculo.pdf');
  const atualizado = fs.existsSync(pdf) && fs.statSync(pdf).mtimeMs >= fs.statSync(BASE_TEX).mtimeMs;
  if (atualizado) return Promise.resolve({ arquivo: pdf });
  if (!compilandoBase) {
    compilandoBase = (async () => {
      fs.mkdirSync(PREVIEW_BASE, { recursive: true });
      fs.copyFileSync(BASE_TEX, path.join(PREVIEW_BASE, 'curriculo.tex'));
      const r = await compilarPdf(PREVIEW_BASE);
      return r.erro ? r : { arquivo: pdf, ...r };
    })().finally(() => { compilandoBase = null; });
  }
  return compilandoBase;
}

function enviarPdf(res, arquivo, nome, baixar) {
  res.writeHead(200, {
    'Content-Type': 'application/pdf',
    'Content-Disposition': `${baixar ? 'attachment' : 'inline'}; filename="${nome}"`,
    'Cache-Control': 'no-store',
  });
  fs.createReadStream(arquivo).pipe(res);
}

function listarSaidas() {
  if (!fs.existsSync(SAIDAS)) return [];
  return fs
    .readdirSync(SAIDAS, { withFileTypes: true })
    .filter((d) => d.isDirectory())
    .map((d) => {
      const abs = path.join(SAIDAS, d.name);
      const rel = path.join(abs, 'relatorio.md');
      let titulo = d.name;
      if (fs.existsSync(rel)) {
        const linha = fs.readFileSync(rel, 'utf8').split(/\r?\n/).find((l) => l.startsWith('# '));
        if (linha) titulo = linha.replace(/^#\s*(Relatório\s*[—-]\s*)?/, '');
      }
      return { pasta: `saidas/${d.name}`, titulo, mtime: fs.statSync(abs).mtimeMs };
    })
    .sort((a, b) => b.mtime - a.mtime);
}

// Pasta criada/alterada depois do início do job (fallback se a linha PASTA_SAIDA faltar)
function pastaMaisRecente(desde) {
  const s = listarSaidas().find((s) => s.mtime >= desde - 1000);
  return s ? pastaSegura(s.pasta) : null;
}

function descreverFerramenta(nome, input = {}) {
  const arq = input.file_path ? path.basename(input.file_path) : '';
  switch (nome) {
    case 'Skill': return 'Carregando as regras do adaptador';
    case 'Read': return /\.(png|jpe?g|webp|gif)$/i.test(arq) ? `Lendo imagem da vaga (${arq})` : `Lendo ${arq}`;
    case 'Write': return `Escrevendo ${arq}`;
    case 'Edit': return `Ajustando ${arq}`;
    case 'Bash': return /pdftotext/.test(input.command || '') ? 'Conferindo o preenchimento das páginas' : 'Compilando o PDF para conferir as 2 páginas';
    case 'Glob':
    case 'Grep': return 'Procurando arquivos';
    default: return nome;
  }
}

const TIPOS_IMAGEM = { 'image/png': 'png', 'image/jpeg': 'jpg', 'image/webp': 'webp', 'image/gif': 'gif' };
const MAX_IMAGENS = 8;

// Salva os prints da vaga (data URLs) em entradas/<id>/ e devolve os caminhos relativos
function salvarImagens(id, imagens = []) {
  if (!Array.isArray(imagens) || !imagens.length) return [];
  if (imagens.length > MAX_IMAGENS) throw new Error(`Envie no máximo ${MAX_IMAGENS} imagens.`);
  const dir = path.join(ROOT, 'entradas', id);
  fs.mkdirSync(dir, { recursive: true });
  return imagens.map((dataUrl, i) => {
    const m = /^data:(image\/[a-z]+);base64,(.+)$/s.exec(dataUrl || '');
    if (!m || !TIPOS_IMAGEM[m[1]]) throw new Error('Formato de imagem não suportado (use PNG, JPG ou WEBP).');
    const nome = `vaga-${i + 1}.${TIPOS_IMAGEM[m[1]]}`;
    fs.writeFileSync(path.join(dir, nome), Buffer.from(m[2], 'base64'));
    return `entradas/${id}/${nome}`;
  });
}

function iniciarJob(titulo, descricao, imagens) {
  const id = crypto.randomUUID();
  const arquivosImagem = salvarImagens(id, imagens);
  const job = { id, status: 'running', log: ['Iniciando o Claude…'], inicio: Date.now(), resposta: '', erro: null, saida: null };
  jobs.set(id, job);
  jobAtivo = id;

  let prompt = `/adaptar-curriculo Título: ${titulo || '(não informado — extraia da vaga)'}\n\nDescrição:\n${descricao || '(ver imagens)'}`;
  if (arquivosImagem.length) {
    prompt += `\n\nImagens da vaga (leia cada uma com a ferramenta Read):\n${arquivosImagem.map((f) => `- ${f}`).join('\n')}`;
  }

  const args = [
    '-p',
    '--output-format', 'stream-json',
    '--verbose',
    '--permission-mode', 'acceptEdits',
    '--allowedTools', 'Skill', 'Read', 'Write', 'Edit', 'Glob', 'Grep', 'Bash(pdflatex *)', 'Bash(pdftotext *)',
  ];
  const argv = WIN ? args.map((a) => (/\s/.test(a) ? `"${a}"` : a)) : args;
  const proc = spawn('claude', argv, { cwd: ROOT, shell: WIN, windowsHide: true });
  proc.stdin.end(prompt);

  let buffer = '';
  let stderr = '';
  proc.stdout.on('data', (chunk) => {
    buffer += chunk.toString('utf8');
    let i;
    while ((i = buffer.indexOf('\n')) >= 0) {
      const linha = buffer.slice(0, i).trim();
      buffer = buffer.slice(i + 1);
      if (!linha) continue;
      let ev;
      try { ev = JSON.parse(linha); } catch { continue; }
      if (ev.type === 'assistant' && ev.message?.content) {
        for (const c of ev.message.content) {
          if (c.type === 'tool_use') job.log.push(descreverFerramenta(c.name, c.input));
          else if (c.type === 'text' && c.text.trim() && job.log.at(-1) !== 'Analisando a vaga…') job.log.push('Analisando a vaga…');
        }
      } else if (ev.type === 'result') {
        job.resposta = ev.result || '';
        if (ev.is_error) job.erro = ev.result || 'O Claude retornou um erro.';
      }
    }
  });
  proc.stderr.on('data', (c) => { stderr += c.toString('utf8'); });

  proc.on('close', async (code) => {
    const m = job.resposta.match(/PASTA_SAIDA:\s*`?([^\s`]+)`?/);
    const abs = (m && pastaSegura(m[1])) || pastaMaisRecente(job.inicio);
    job.resposta = job.resposta.replace(/\n?.*PASTA_SAIDA:.*$/m, '').trim();
    if (code !== 0 && !job.erro) job.erro = stderr.trim() || `O processo terminou com código ${code}.`;
    if (abs && !job.erro) {
      job.log.push('Gerando o PDF');
      const pdf = await compilarPdf(abs);
      if (pdf.erro) job.avisoPdf = pdf.erro;
    }
    if (abs) job.saida = lerSaida(abs);
    if (!job.saida?.tex && !job.erro) job.erro = 'O Claude terminou, mas não encontrei o curriculo.tex gerado.';
    if (/not logged in|\/login|invalid api key|authentication/i.test(`${job.erro} ${job.resposta}`)) {
      job.erro = 'O Claude Code não está autenticado.\nConfigure CLAUDE_CODE_OAUTH_TOKEN (ou ANTHROPIC_API_KEY) no arquivo .env e reinicie o container. Veja o README.';
      job.resposta = '';
    }
    job.status = job.erro ? 'error' : 'done';
    job.log.push(job.erro ? 'Falhou' : 'Pronto');
    jobAtivo = null;
  });
  proc.on('error', (e) => {
    jobAtivo = null;
    job.status = 'error';
    job.erro = `Não consegui executar o comando "claude": ${e.message}`;
  });

  return job;
}

function autorizado(req) {
  if (!SENHA) return true;
  const [tipo, valor] = (req.headers.authorization || '').split(' ');
  if (tipo !== 'Basic' || !valor) return false;
  const senha = Buffer.from(valor, 'base64').toString().split(':').slice(1).join(':');
  const a = Buffer.from(senha), b = Buffer.from(SENHA);
  return a.length === b.length && crypto.timingSafeEqual(a, b);
}

const server = http.createServer(async (req, res) => {
  if (!autorizado(req)) {
    res.writeHead(401, { 'WWW-Authenticate': 'Basic realm="Adaptador de Curriculo"' });
    return res.end('Senha necessária');
  }
  const url = new URL(req.url, `http://${req.headers.host}`);
  try {
    if (req.method === 'GET' && url.pathname === '/') {
      res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
      return res.end(fs.readFileSync(path.join(ROOT, 'ui', 'index.html')));
    }
    if (req.method === 'POST' && url.pathname === '/api/adaptar') {
      if (jobAtivo) return json(res, 409, { erro: 'Já existe uma adaptação em andamento.', id: jobAtivo });
      const { titulo = '', descricao = '', imagens = [] } = JSON.parse(await lerBody(req));
      if (!descricao.trim() && !imagens.length) return json(res, 400, { erro: 'Cole a descrição da vaga ou envie um print dela.' });
      const job = iniciarJob(titulo.trim(), descricao.trim(), imagens);
      return json(res, 200, { id: job.id });
    }
    if (req.method === 'GET' && url.pathname.startsWith('/api/status/')) {
      const job = jobs.get(url.pathname.split('/').pop());
      if (!job) return json(res, 404, { erro: 'Job não encontrado.' });
      return json(res, 200, { ...job, segundos: Math.round((Date.now() - job.inicio) / 1000) });
    }
    if (req.method === 'GET' && url.pathname === '/api/historico') {
      return json(res, 200, listarSaidas());
    }
    if (req.method === 'GET' && url.pathname === '/api/saida') {
      const abs = pastaSegura(url.searchParams.get('pasta'));
      if (!abs) return json(res, 404, { erro: 'Pasta não encontrada.' });
      return json(res, 200, lerSaida(abs));
    }
    if (req.method === 'POST' && url.pathname === '/api/compilar') {
      const abs = pastaSegura(url.searchParams.get('pasta'));
      if (!abs) return json(res, 404, { erro: 'Pasta não encontrada.' });
      const pdf = await compilarPdf(abs);
      if (pdf.erro) return json(res, 500, { erro: pdf.erro });
      return json(res, 200, lerSaida(abs));
    }
    if (req.method === 'GET' && url.pathname === '/api/pdf') {
      const abs = pastaSegura(url.searchParams.get('pasta'));
      const arq = abs && path.join(abs, 'curriculo.pdf');
      if (!arq || !fs.existsSync(arq)) return json(res, 404, { erro: 'PDF não encontrado.' });
      return enviarPdf(res, arq, `curriculo-${path.basename(abs)}.pdf`, url.searchParams.has('baixar'));
    }
    // Salva o LaTeX editado no editor e recompila
    if (req.method === 'POST' && url.pathname === '/api/salvar') {
      const abs = pastaSegura(url.searchParams.get('pasta'));
      if (!abs) return json(res, 404, { erro: 'Pasta não encontrada.' });
      const { tex } = JSON.parse(await lerBody(req));
      if (typeof tex !== 'string' || !tex.includes('\\begin{document}')) return json(res, 400, { erro: 'LaTeX inválido.' });
      fs.writeFileSync(path.join(abs, 'curriculo.tex'), tex);
      const pdf = await compilarPdf(abs);
      return json(res, 200, { ...lerSaida(abs), erroCompilacao: pdf.erro || null });
    }
    if (req.method === 'GET' && url.pathname === '/api/base') {
      let paginas = null;
      try { paginas = JSON.parse(fs.readFileSync(path.join(PREVIEW_BASE, 'pdf.json'), 'utf8')).paginas; } catch {}
      return json(res, 200, { tex: fs.readFileSync(BASE_TEX, 'utf8'), paginas });
    }
    if (req.method === 'GET' && url.pathname === '/api/base-pdf') {
      const r = await pdfBase();
      if (r.erro) return json(res, 500, { erro: r.erro });
      return enviarPdf(res, r.arquivo, 'curriculo-base.pdf', url.searchParams.has('baixar'));
    }
    json(res, 404, { erro: 'Não encontrado' });
  } catch (e) {
    json(res, 500, { erro: e.message });
  }
});

server.listen(PORT, HOST, () => {
  console.log(`Adaptador de currículo rodando em http://localhost:${PORT}`);
});
