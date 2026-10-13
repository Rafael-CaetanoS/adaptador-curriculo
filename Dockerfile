# ---------- Build (.NET SDK) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/AdaptadorCurriculo/AdaptadorCurriculo.csproj AdaptadorCurriculo/
RUN dotnet restore AdaptadorCurriculo/AdaptadorCurriculo.csproj
COPY src/AdaptadorCurriculo/ AdaptadorCurriculo/
RUN dotnet publish AdaptadorCurriculo/AdaptadorCurriculo.csproj -c Release -o /out --no-restore

# ---------- Runtime (ASP.NET + LaTeX) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# LaTeX (pdflatex, mesmo compilador do Overleaf) + pacotes usados pelo currículo
# (titlesec, enumitem, fontawesome5, babel português), fontes vetoriais (cm-super)
# e pdftotext para conferir o preenchimento das páginas.
# Fica antes do COPY do app para o cache do Docker não refazer esta etapa a cada mudança no código.
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
      texlive-latex-base texlive-latex-recommended texlive-latex-extra \
      texlive-fonts-recommended texlive-fonts-extra texlive-lang-portuguese \
      cm-super poppler-utils \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /out .
COPY prompts/ ./prompts/

# DADOS_DIR: pasta do projeto montada pelo docker-compose (currículo, perfil, saídas, prompts)
ENV ASPNETCORE_URLS=http://0.0.0.0:3210 \
    DADOS_DIR=/dados
USER app
EXPOSE 3210
ENTRYPOINT ["dotnet", "AdaptadorCurriculo.dll"]
