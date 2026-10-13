# Adaptador de currículo (ATS)

Projeto para adaptar o currículo do Rafael para vagas específicas. Ver `README.md`.

- `curriculo-base.tex`: currículo mestre (LaTeX/Overleaf). Só altere quando o usuário pedir explicitamente para atualizar o currículo base.
- `perfil-extra.md`: experiências/tecnologias reais que não estão no currículo.
- `saidas/`: um subdiretório por vaga com `curriculo.tex`, `curriculo.pdf`, `relatorio.md`, `vaga.md`.
- Backend: ASP.NET Core 10 em `src/AdaptadorCurriculo` (Minimal API, `Microsoft.Extensions.AI` com cliente OpenAI-compatível — qualquer provedor). Página em `wwwroot/index.html`.
- Orquestração em `Servicos/OrquestradorAdaptacao.cs`: prompt (`prompts/sistema.md`) → IA → pdflatex → confere 2 páginas e trava "não inventar" (`prompts/tecnologias.txt`) → até 3 rodadas de correção.
- Roda via Docker (`docker compose up -d --build`, porta 3210); a pasta do projeto é montada em `/dados`. O usuário não quer instalar nada no PC além do Docker.
- `.claude/skills/adaptar-curriculo/SKILL.md`: versão da mesma tarefa para rodar direto no Claude Code.

Regras fixas: nunca inventar experiência; currículo sempre com 2 páginas cheias (layout de `prompts/layout-referencia.tex`).
Próximas fases planejadas: vários usuários (login + área por pessoa) e servidor MCP.
