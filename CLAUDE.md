# Adaptador de currículo (ATS)

Projeto para adaptar o currículo do Rafael para vagas específicas. Ver `README.md`.

- `curriculo-base.tex`: currículo mestre (LaTeX/Overleaf). Só altere quando o usuário pedir explicitamente para atualizar o currículo base.
- `perfil-extra.md`: experiências/tecnologias reais que não estão no currículo.
- `saidas/`: um subdiretório por vaga com `curriculo.tex`, `curriculo.pdf`, `relatorio.md`, `vaga.md`.
- Comando principal: `/adaptar-curriculo` (ver `.claude/skills/adaptar-curriculo/SKILL.md`).
- Interface web: `server.js` + `ui/index.html`, rodando via Docker (`docker compose up -d --build`, porta 3210). O usuário não quer instalar nada no PC além do Docker.

Regras fixas: nunca inventar experiência; currículo sempre com 2 páginas cheias (layout padrão do SKILL.md).
