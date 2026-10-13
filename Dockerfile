# ---------- Build (.NET SDK) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/AdaptadorCurriculo/AdaptadorCurriculo.csproj AdaptadorCurriculo/
RUN dotnet restore AdaptadorCurriculo/AdaptadorCurriculo.csproj
COPY src/AdaptadorCurriculo/ AdaptadorCurriculo/
RUN dotnet publish AdaptadorCurriculo/AdaptadorCurriculo.csproj -c Release -o /out --no-restore

# ---------- Claude Code CLI (para usar a assinatura do Claude) ----------
FROM node:22-bookworm-slim AS claude
RUN npm install -g @anthropic-ai/claude-code

# ---------- Runtime (ASP.NET + LaTeX mínimo) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Ferramentas do sistema: perl (tlmgr), pdftotext (conferir páginas)
RUN apt-get update \
 && apt-get install -y --no-install-recommends ca-certificates wget perl xz-utils fontconfig poppler-utils \
 && rm -rf /var/lib/apt/lists/*

# LaTeX mínimo (TinyTeX) só com os pacotes que o currículo usa.
# Bem mais leve que o TeX Live do apt (~2 GB): mesmo pdflatex do Overleaf, fontes vetoriais (cm-super).
# Fica antes do COPY do app para o cache do Docker não refazer esta etapa a cada mudança no código.
RUN arq=$(uname -m | sed 's/aarch64/arm64/') \
 && wget -qO- "https://github.com/rstudio/tinytex-releases/releases/download/daily/TinyTeX-1-linux-${arq}.tar.xz" | tar -xJ -C /opt \
 && /opt/.TinyTeX/bin/*/tlmgr option sys_bin /usr/local/bin \
 && /opt/.TinyTeX/bin/*/tlmgr path add \
 && tlmgr install titlesec enumitem fontawesome5 babel-portuges hyphen-portuguese ec cm-super \
      geometry hyperref xcolor tools \
 && tlmgr path add \
 && chmod -R a+rX /opt/.TinyTeX \
 && rm -rf /opt/.TinyTeX/texmf-dist/doc /opt/.TinyTeX/texmf-var/web2c/*.log

# Claude Code CLI (executável nativo)
COPY --from=claude /usr/local/lib/node_modules/@anthropic-ai /usr/local/lib/node_modules/@anthropic-ai
RUN ln -s /usr/local/lib/node_modules/@anthropic-ai/claude-code/bin/claude.exe /usr/local/bin/claude

WORKDIR /app
COPY --from=build /out .
COPY prompts/ ./prompts/

# DADOS_DIR: pasta do projeto montada pelo docker-compose (currículo, perfil, saídas, prompts)
# CLAUDE_CONFIG_DIR: configuração do Claude Code num lugar gravável pelo usuário "app"
ENV ASPNETCORE_URLS=http://0.0.0.0:3210 \
    DADOS_DIR=/dados \
    CLAUDE_CONFIG_DIR=/home/app/.claude \
    DISABLE_AUTOUPDATER=1
USER app
EXPOSE 3210
ENTRYPOINT ["dotnet", "AdaptadorCurriculo.dll"]
