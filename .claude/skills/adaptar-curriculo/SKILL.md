---
name: adaptar-curriculo
description: Adapta o currículo LaTeX (curriculo-base.tex) para uma vaga específica, otimizando para filtros ATS com os termos exatos da descrição da vaga, e gera um relatório de compatibilidade com o que falta. Use quando o usuário colar uma descrição de vaga ou pedir para adaptar o currículo.
argument-hint: "Título: <cargo da vaga> / Descrição: <texto da vaga>"
---

# Adaptar currículo para uma vaga (ATS)

Descrição da vaga recebida:

$ARGUMENTS

Se a descrição acima estiver vazia, peça ao usuário para colar a descrição da vaga e pare até recebê-la.

Se a entrada trouxer uma linha `Título:`, esse é o **título exato do cargo**: trate como o termo
obrigatório de maior peso, use no cabeçalho e na 1ª frase do resumo (respeitando a regra de
senioridade) e no nome da pasta de saída. Se vier `(não informado — extraia da vaga)`, extraia
o título exato do texto ou das imagens.

**Imagens da vaga:** se a entrada listar arquivos de imagem (prints do LinkedIn, Gupy etc.),
leia **todas** com a ferramenta Read antes do Passo 2 e transcreva o texto da vaga fielmente,
mantendo a grafia exata dos termos (é isso que o ATS compara). Junte com a descrição em texto,
se houver, sem duplicar trechos. Ignore elementos de interface (botões, menus, "Candidatar-se",
vagas sugeridas). Se algum trecho estiver ilegível ou cortado, diga no relatório em "Atenção".

**Sempre** salve a vaga completa (texto colado + transcrição das imagens) em
`saidas/<pasta>/vaga.md`, com o título na primeira linha (`# <Título>`).

## Contexto

Sistemas ATS (Gupy, Kenoby, Workday, Greenhouse etc.) filtram currículos procurando
os **termos exatos** da vaga. "Dev .NET" não casa com "Desenvolvedor .NET"; "CI/CD"
pode não casar com "Integração Contínua". O objetivo é reescrever o currículo usando
o vocabulário da vaga **sem inventar nada**.

## Arquivos

- `curriculo-base.tex` — currículo mestre. **Nunca edite este arquivo.**
- `perfil-extra.md` — tecnologias/experiências reais do usuário que não estão no currículo. Pode ser usado como fonte.
- Saída: `saidas/<AAAA-MM-DD>-<empresa>-<cargo>/` (slug minúsculo, sem acentos, hífens). Se a empresa não aparecer na vaga, use `vaga`. Se a pasta já existir, acrescente `-2`, `-3`… (nunca sobrescreva uma saída anterior).

## Passo 1 — Ler

Leia `curriculo-base.tex` e `perfil-extra.md` inteiros.

## Passo 2 — Extrair palavras-chave da vaga

Monte uma lista com, para cada termo, a **grafia exata usada na vaga**:

- **Título do cargo** (ex.: "Desenvolvedor .NET Sênior", "Engenheiro de Software Back-end")
- **Hard skills**: linguagens, frameworks, bancos, cloud, ferramentas, padrões, metodologias
- **Requisitos de experiência** (anos, domínio, setor)
- **Soft skills / responsabilidades** citadas (ex.: "trabalho em equipe", "mentoria")
- **Formação, idiomas, certificações**

Classifique cada termo como **Obrigatório** (requisitos, "necessário", "imprescindível")
ou **Diferencial** ("desejável", "plus", "diferencial"). Na dúvida, Obrigatório.
Termos repetidos várias vezes na vaga têm peso maior.

## Passo 3 — Casar com o currículo

Para cada termo, classifique:

| Status | Significado | Ação |
|---|---|---|
| ✅ Exato | aparece com a mesma grafia | manter, garantir destaque |
| 🔁 Equivalente | o usuário tem, mas escrito diferente (sinônimo, sigla, versão, subconjunto óbvio) | **trocar/complementar para a grafia da vaga** |
| 📎 No perfil-extra | está em `perfil-extra.md` | pode adicionar ao currículo |
| ❌ Ausente | não há evidência | **NÃO adicionar** — vai para o relatório |

Exemplos de equivalência válida: ".NET Core"/"ASP.NET Core"; "C#" implícito em .NET
(pode adicionar C# — quem trabalha com ASP.NET Core programa em C#); "CI/CD" ↔ "Integração
e Entrega Contínua"; "Azure DevOps Pipelines" ↔ "pipelines no Azure DevOps"; "Microsserviços"
quando há Service Bus + desacoplamento entre serviços (marque como equivalente fraco e cite no
relatório); "API RESTful" ↔ "APIs REST"; "SQL" ↔ SQL Server/PostgreSQL; "Git" ↔ "GitHub/Azure Repos"
só se fizer sentido; "testes automatizados" ↔ xUnit; "liderança técnica"/"Tech Lead" ↔ coordenação.

Equivalências **inválidas** (são ❌): React ≠ Angular; AWS ≠ Azure; Kubernetes ≠ Docker;
RabbitMQ/Kafka ≠ Azure Service Bus; Python ≠ Java; MongoDB ≠ SQL. Não trate uma tecnologia
"parecida" como equivalente.

## Passo 4 — Gerar o currículo adaptado

Copie `curriculo-base.tex` e altere **apenas conteúdo textual**, mantendo o preâmbulo e o layout.

**Pode:**
1. **Título abaixo do nome**: usar o título do cargo da vaga, desde que compatível com a
   experiência real. Não inflar senioridade: se a vaga diz "Sênior" e o currículo não sustenta
   claramente, use o título sem o nível (ex.: "Desenvolvedor .NET") e cite no relatório.
2. **Linha de tecnologias do cabeçalho**: reordenar/trocar para as 5–7 tecnologias mais
   importantes da vaga que o usuário tem.
3. **Objetivo**: não existe no layout padrão (ver "Tamanho: 2 páginas cheias").
4. **Resumo Profissional**: incluir naturalmente os termos-chave obrigatórios que o usuário tem,
   com a grafia da vaga. Primeira frase com o título do cargo da vaga.
5. **Bullets de experiência**: trocar termos pela grafia da vaga, reordenar bullets dentro de cada
   cargo (mais relevantes primeiro), ajustar redação para refletir responsabilidades da vaga
   **que o usuário de fato exerceu**. Pode adicionar um bullet só se houver base em
   `perfil-extra.md` ou se for explicitação direta de algo já presente.
6. **Competências Técnicas**: reordenar categorias e itens por relevância; usar a grafia da vaga;
   para siglas, incluir a forma longa uma vez (ex.: "CI/CD (Integração e Entrega Contínua)").
   Adicionar itens 🔁 e 📎.
7. **Projetos**: reordenar por relevância e ajustar termos.
8. Ajustar o tamanho conforme "Tamanho: 2 páginas cheias".

## Tamanho: 2 páginas cheias (OBRIGATÓRIO)

O PDF gerado no Overleaf deve **ocupar as 2 páginas quase inteiras, sem passar para a 3ª**.
Nem compacto demais (o usuário achou 1,5 página "estranho de ler"), nem passando de 2.
Escreva já mirando no orçamento abaixo e depois faça a "Conferência real" (A4, 11pt, margens
1.4cm ≈ 100 caracteres por linha, ≈ 55 linhas por página): mire em **~100–105 linhas estimadas**.

**Layout padrão (arquivo de referência: `saidas/2026-10-13-vaga-desenvolvedor-net-pleno/curriculo.tex`)**.
Copie a estrutura desse arquivo:
- Preâmbulo igual ao base + `\titlespacing*{\section}{0pt}{14pt}{7pt}` logo após o `\titleformat`
  (com o comentário explicando que diminuir/aumentar ajusta o tamanho).
- **Sem seção "Objetivo"**: o título do cargo vai no cabeçalho e na 1ª frase do resumo.
- **Resumo Profissional**: 2 parágrafos (~4 linhas cada) separados por `\vspace{0.15cm}`.
- **Cargos**: `\textbf{Cargo} \hfill Data`, empresa na linha de baixo, `\vspace{0.2cm}` entre cargos.
- **Bullets**: cargo atual 7–8, demais cargos 4–5. Cada bullet com no máximo 2 linhas.
- **Projetos**: os principais com título em linha própria + descrição; os menores em 1 linha
  (`\textbf{Nome} -- descrição`); `\vspace{0.2cm}` entre eles.
- **Competências**: `\textbf{Categoria:} itens`, uma por linha, `\vspace{0.12cm}` entre elas.
- **Formação**: curso + data na 1ª linha, instituição na 2ª, `\vspace{0.2cm}` entre cursos.
- **Idiomas**: tudo em 1 linha, separado por `\quad|\quad`.

Ajuste fino: se o conteúdo da vaga gerar mais texto, corte nesta ordem: itens de baixa
relevância nas competências, projetos menos relevantes, bullets do estágio, e por último
encurte bullets. Se gerar menos, devolva bullets/detalhes relevantes do `curriculo-base.tex`.
Nunca corte termos-chave obrigatórios da vaga.

**Conferência real (quando `pdflatex` estiver disponível, como no Docker):** depois de salvar o
`curriculo.tex`, compile e confira de verdade, em vez de só estimar:

```
pdflatex -interaction=nonstopmode -halt-on-error -output-directory saidas/<pasta> saidas/<pasta>/curriculo.tex
pdftotext -f 2 -l 2 -layout saidas/<pasta>/curriculo.pdf -
```

- A saída do pdflatex diz `Output written on ... (N pages)`. Precisa ser **N = 2**.
- O `pdftotext` da página 2 deve ter **pelo menos ~40 linhas de texto** (página bem preenchida).
  Se tiver muito menos, devolva conteúdo relevante do base ou aumente o `\titlespacing`/`\vspace`.
- Se N = 3, corte conforme a ordem acima ou reduza os espaçamentos. Se N = 1, devolva conteúdo.
- Repita no máximo 3 vezes. Se der erro de compilação, corrija o LaTeX.

No relatório, informe o número de páginas (real, se compilou; senão, estimado e peça para
o usuário confirmar no Overleaf).

**Nunca:**
- Inventar tecnologia, experiência, certificação, número ou resultado.
- Alterar nomes de empresas, títulos dos cargos ocupados, datas, formação, nível de idioma ou contatos.
- Fazer "keyword stuffing" (lista de palavras escondida, texto branco, repetição artificial).
- Mudar o preâmbulo/pacotes, exceto o `\titlespacing` do layout padrão e o caso de idioma abaixo.

**Idioma:** se a vaga estiver em inglês, gere o currículo inteiro em inglês
(trocar `\usepackage[brazil]{babel}` por `\usepackage[english]{babel}`, traduzir seções,
datas "Aug/2025 -- Present" etc.). Nível de inglês continua "Basic" — não altere.

**LaTeX:** escape caracteres especiais (`C\#`, `\&`, `\%`, `\_`, `\$`). Confira que todo
`\begin{...}` tem `\end{...}` e que as chaves estão balanceadas. Se `pdflatex` estiver disponível
(`pdflatex --version`), compile em um diretório temporário para validar; senão, revise com cuidado.

Salve como `saidas/<pasta>/curriculo.tex`.

## Passo 5 — Relatório

Salve `saidas/<pasta>/relatorio.md` em português com:

```markdown
# Relatório — <Cargo> @ <Empresa>

## Compatibilidade
- Antes: X% dos obrigatórios / Y% dos diferenciais
- Depois: X% dos obrigatórios / Y% dos diferenciais
(% = termos com status ✅ no currículo / total da categoria)

## Palavras-chave da vaga
| Termo (grafia da vaga) | Tipo | Antes | Depois | Observação |

## O que foi alterado
- Seção — antes → depois — motivo (lista objetiva)

## O que falta (❌)
### Obrigatórios
- termo — por que importa — sugestão (estudar, projeto pessoal, se você tem: adicionar ao perfil-extra.md)
### Diferenciais
- ...

## Atenção
- Equivalências fracas que podem ser questionadas em entrevista
- Requisitos de anos de experiência / senioridade / idioma não atendidos
- Qualquer outro risco (ex.: vaga exige inglês avançado)

## Perguntas para você
- "Você já usou X? Se sim, adicione ao perfil-extra.md e rode de novo."
```

## Passo 6 — Responder no chat

Resposta curta:
1. Compatibilidade antes → depois (obrigatórios e diferenciais)
2. Top 3–5 alterações
3. O que falta (obrigatórios primeiro)
4. Perguntas sobre itens que o usuário talvez tenha
5. Links para `curriculo.tex` e `relatorio.md`, e lembrete: copiar o conteúdo de `curriculo.tex`
   para o Overleaf (ou fazer upload do arquivo) e gerar o PDF.
6. **Última linha da resposta, sempre, exatamente neste formato** (a interface web usa isso):
   `PASTA_SAIDA: saidas/<pasta>`
