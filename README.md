# Adaptador de Currículo (ATS)

Cole a descrição (ou prints) de uma vaga e receba o currículo reescrito com os **termos exatos
da vaga**, em **PDF de 2 páginas**, mais um relatório do que falta. Nada é inventado: só o que
está em `curriculo-base.tex` e `perfil-extra.md` entra no currículo.

Tudo roda dentro de um container Docker (Node + Claude Code + LaTeX). Não é preciso instalar
nada além do Docker.

## Primeira vez

1. **Autenticação do Claude** (uma vez só). Gere um token da sua assinatura:

   ```bash
   docker compose run --rm adaptador claude setup-token
   ```

   Abra o link que aparecer, autorize e copie o token gerado.

2. **Crie o `.env`** a partir do `.env.example` e cole o token em `CLAUDE_CODE_OAUTH_TOKEN=`.
   (Alternativa: `ANTHROPIC_API_KEY=` com uma chave da API, cobrada por uso.)

3. **Suba:**

   ```bash
   docker compose up -d --build
   ```

4. Abra **http://localhost:3210**.

## Dia a dia

```bash
docker compose up -d      # liga
docker compose down       # desliga
docker compose logs -f    # vê os logs
```

No Windows, dá para clicar duas vezes em `iniciar.bat`.

Em cada vaga, a página tem as abas:
- **Editor**: código LaTeX à esquerda e PDF à direita, como no Overleaf. `Ctrl+S` salva e recompila.
- **Comparar**: PDF da versão salva (`curriculo-base.tex`) ao lado da versão adaptada, com rolagem sincronizada.
- **Diferenças**: o que mudou no código, linha a linha, lado a lado.

## Onde ficam as coisas

| Arquivo/pasta | O que é |
|---|---|
| `curriculo-base.tex` | Seu currículo mestre. Edite aqui quando mudar de emprego, etc. |
| `perfil-extra.md` | Coisas reais que você sabe/fez e não estão no currículo. |
| `saidas/<data>-<empresa>-<cargo>/` | `curriculo.pdf`, `curriculo.tex`, `relatorio.md`, `vaga.md` |
| `entradas/` | Prints de vagas enviados pela página |
| `.claude/skills/adaptar-curriculo/SKILL.md` | As regras do adaptador (ATS, 2 páginas, honestidade) |

Esses arquivos ficam fora do container (volumes), então sobrevivem a `docker compose down` e rebuilds.

## Rodar em um servidor

Em `docker-compose.yml`, troque `"127.0.0.1:3210:3210"` por `"3210:3210"` e **defina
`APP_PASSWORD` no `.env`** — sem senha, qualquer pessoa com o endereço usaria sua conta Claude
e veria seu currículo. Para acesso pela internet, coloque atrás de HTTPS (Caddy, Nginx, Cloudflare Tunnel).
