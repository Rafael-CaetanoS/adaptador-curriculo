# Adaptador de Currículo (ATS)

Cole a descrição (ou prints) de uma vaga e receba o currículo reescrito com os **termos exatos
da vaga**, em **PDF de 2 páginas**, mais um relatório do que falta. Nada é inventado: só o que
está em `curriculo-base.tex` e `perfil-extra.md` entra no currículo, e o sistema confere isso em código.

Funciona com **qualquer IA compatível com a API do OpenAI**: OpenAI, Claude (Anthropic), Google
Gemini, OpenRouter, Groq, DeepSeek, Mistral, Ollama e LM Studio (locais e gratuitos), entre outras.
Cada pessoa configura a própria IA e chave na página.

Tudo roda dentro de um container Docker (ASP.NET Core 10 + LaTeX). Não é preciso instalar nada
além do Docker.

## Subir

```bash
docker compose up -d --build
```

Abra **http://localhost:3210**, clique em **Configurar IA**, escolha a plataforma, cole a chave e
use **Testar conexão**. A configuração fica salva só no seu navegador.

Opcional: para ter uma IA padrão do servidor (usada quando a página não manda configuração),
copie `.env.example` para `.env` e preencha `IA_BASE_URL`, `IA_MODELO` e `IA_API_KEY`.

### Usar a assinatura do Claude (Pro/Max)

Além das APIs, dá para usar a sua assinatura do Claude, pelo Claude Code que vem dentro do container:

```bash
docker compose run --rm --entrypoint claude adaptador setup-token
```

Coloque o token em `CLAUDE_CODE_OAUTH_TOKEN=` no `.env` (numa linha só, sem espaços) e escolha
**Assinatura Claude (Claude Code)** no painel. A assinatura é pessoal: se outras pessoas usarem a
página, cada uma deve usar a própria chave ou o próprio token.

## Como funciona

```
Página → ASP.NET Core (orquestrador) → IA (qualquer plataforma)
             ├─ monta o prompt: prompts/sistema.md + currículo + perfil + layout + vaga (texto/prints)
             ├─ recebe currículo LaTeX + relatório
             ├─ compila com pdflatex e confere: 2 páginas? 2ª página cheia?
             ├─ trava "não inventar": tecnologias de prompts/tecnologias.txt sem base no currículo/perfil
             └─ se algo falhar, devolve os problemas para a IA corrigir (até 3 rodadas)
```

Em cada vaga, a página tem as abas:
- **PDF**: o currículo final, com botão para baixar.
- **Editor**: código LaTeX à esquerda e PDF à direita, como no Overleaf. `Ctrl+S` salva e recompila.
- **Comparar**: PDF da versão salva (`curriculo-base.tex`) ao lado da versão adaptada, com rolagem sincronizada.
- **Diferenças**: o que mudou no código, linha a linha.
- **Resumo**, **Relatório** e **Vaga**.

## Dia a dia

```bash
docker compose up -d      # liga
docker compose down       # desliga
docker compose logs -f    # vê os logs
```

No Windows, dá para clicar duas vezes em `iniciar.bat`.

## Onde ficam as coisas

| Arquivo/pasta | O que é |
|---|---|
| `curriculo-base.tex` | Seu currículo mestre. |
| `perfil-extra.md` | Coisas reais que você sabe/fez e não estão no currículo. |
| `saidas/<data>-<empresa>-<cargo>/` | `curriculo.pdf`, `curriculo.tex`, `relatorio.md`, `vaga.md` |
| `prompts/sistema.md` | Regras que a IA segue (ATS, 2 páginas, honestidade). Editável sem rebuild. |
| `prompts/tecnologias.txt` | Lista da trava "não inventar". Editável sem rebuild. |
| `prompts/layout-referencia.tex` | Modelo de layout que a IA copia. |
| `src/AdaptadorCurriculo/` | Backend ASP.NET Core + página (`wwwroot/index.html`). |
| `.claude/skills/adaptar-curriculo/` | Versão para usar direto no Claude Code (`/adaptar-curriculo`). |

A pasta do projeto é montada no container em `/dados`, então currículo, perfil, saídas e prompts
ficam no seu computador e sobrevivem a rebuilds.

## Desenvolvimento sem Docker

```bash
dotnet run --project src/AdaptadorCurriculo
```

Sobe em http://localhost:5066. Sem `pdflatex` instalado, a adaptação funciona mas o PDF não é gerado.

## Rodar em um servidor

Em `docker-compose.yml`, troque `"127.0.0.1:3210:3210"` por `"3210:3210"` e **defina
`APP_PASSWORD` no `.env`**. Coloque atrás de HTTPS (Caddy, Nginx, Cloudflare Tunnel): a chave de
API de quem usa a página trafega em cada adaptação.
