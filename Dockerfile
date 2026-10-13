FROM node:22-bookworm-slim

# LaTeX (pdflatex, mesmo compilador do Overleaf) + pacotes usados pelo currículo
# (titlesec, enumitem, fontawesome5, babel português) + pdftotext para conferir as páginas.
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
      ca-certificates git \
      texlive-latex-base texlive-latex-recommended texlive-latex-extra \
      texlive-fonts-recommended texlive-fonts-extra texlive-lang-portuguese \
      poppler-utils \
 && rm -rf /var/lib/apt/lists/*

# Fontes vetoriais (Type 1), como no Overleaf. Sem isso o PDF sai com fontes em bitmap,
# borradas e piores para o ATS extrair o texto.
RUN apt-get update \
 && apt-get install -y --no-install-recommends cm-super \
 && rm -rf /var/lib/apt/lists/*

# Claude Code CLI (roda o /adaptar-curriculo em modo headless)
RUN npm install -g @anthropic-ai/claude-code

WORKDIR /app
COPY --chown=node:node . .
RUN mkdir -p saidas entradas /home/node/.claude && chown -R node:node /app /home/node/.claude

USER node
ENV HOST=0.0.0.0 \
    PORT=3210
EXPOSE 3210
CMD ["node", "server.js"]
