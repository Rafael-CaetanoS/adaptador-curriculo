# Relatório — Desenvolvedor .NET PL @ Mirante Tecnologia

## Compatibilidade
- Antes: 39% dos obrigatórios (14/36) / diferenciais: a vaga não tem seção de diferenciais
- Depois: 69% dos obrigatórios (25/36) / diferenciais: n/a
(% = termos com status ✅ no currículo / total da categoria. A vaga não separa diferenciais, então tudo foi tratado como obrigatório.)

> Contexto: é uma vaga "guarda-chuva" de consultoria. Mistura stack .NET (C#, .NET Core, Angular, Blazor) com stack Java EE (JPA, JSF, JEE) e ferramentas (Jenkins, Sonar, Jira, ServiceNow). Dificilmente alguém tem tudo. O núcleo da vaga (**C# .NET Core + Angular + Blazor + SQL Server/Oracle + CI/CD + Docker**) você cobre inteiro.

## Palavras-chave da vaga
| Termo (grafia da vaga) | Tipo | Antes | Depois | Observação |
|---|---|---|---|---|
| Desenvolvedor .NET PL | Obrig. | 🔁 | ✅ | "Desenvolvedor .NET Pleno" no cabeçalho e na 1ª frase do resumo |
| C# | Obrig. | 🔁 | ✅ | implícito em ASP.NET Core |
| .NET Core | Obrig. | 🔁 | ✅ | era só "ASP.NET Core" |
| Angular | Obrig. | ✅ | ✅ | |
| Blazor | Obrig. | ✅ | ✅ | também no cabeçalho e nas competências |
| TypeScript | Obrig. | ✅ | ✅ | |
| JavaScript | Obrig. | ✅ | ✅ | |
| Bootstrap | Obrig. | 📎 | ✅ | informado por você (perfil-extra) |
| NodeJs | Obrig. | ❌ | ❌ | |
| automação Python | Obrig. | ❌ | ❌ | |
| Agentes IA | Obrig. | 📎 | ✅ | equivalente fraco: uso de Claude/ChatGPT (não construção de agentes) |
| Padrões de Projeto (Design Patterns) | Obrig. | ✅ | ✅ | agora com as duas grafias |
| Orientação a Objetos | Obrig. | 🔁 | ✅ | base: SOLID, C#, Java |
| UML | Obrig. | 📎 | ✅ | informado por você |
| Integração (CI/CD) | Obrig. | ✅ | ✅ | |
| JPA | Obrig. | 📎 | ✅ | Spring Data JPA |
| JSF | Obrig. | ❌ | ❌ | |
| Hibernate | Obrig. | ✅ | ✅ | |
| Desenvolvimento JEE | Obrig. | ❌ | ❌ | tem Java, mas não JEE |
| Boas Práticas em Desenvolvimento de software | Obrig. | 🔁 | ✅ | era "boas práticas de engenharia de software" |
| APIs REST | Obrig. | ✅ | ✅ | |
| SOAP | Obrig. | ❌ | ❌ | |
| GraphQl | Obrig. | ❌ | ❌ | |
| Microsserviços | Obrig. | 🔁 fraco | ✅ | equivalente fraco: vem de Service Bus + integração entre sistemas |
| Oracle | Obrig. | ✅ | ✅ | |
| SQL Server | Obrig. | ✅ | ✅ | |
| SQL | Obrig. | ✅ | ✅ | |
| NoSQL | Obrig. | ❌ | ❌ | |
| Jenkins | Obrig. | ❌ | ❌ | você usa Azure DevOps, que não é equivalente |
| Sonar | Obrig. | ❌ | ❌ | |
| Docker | Obrig. | ✅ | ✅ | |
| Jira | Obrig. | ❌ | ❌ | |
| ServiceNow | Obrig. | ❌ | ❌ | |
| Github | Obrig. | 🔁 | ✅ | perfil GitHub + Git |
| Teste Unitários | Obrig. | ✅ | ✅ | |
| Graduação em TI | Obrig. | ✅ | ✅ | Sistemas de Informação |

## O que foi alterado
- **Novo destaque (economia de R$ 6 mil/mês)**: entrou em 3 lugares: 1º bullet do cargo atual (em negrito, com ~R$ 72 mil/ano), última frase do 1º parágrafo do resumo e um novo projeto, "Consolidação de Arquitetura". Também foi registrado no `perfil-extra.md` para as próximas vagas.
- Cabeçalho: "Engenheiro de Software Full-Stack" → "Desenvolvedor .NET Pleno"; tecnologias → C# • .NET Core • Angular • Blazor • SQL Server • Oracle • Docker • CI/CD.
- Objetivo: removido (layout padrão).
- Resumo: reescrito com a grafia da vaga (C# .NET Core, Angular, Blazor, TypeScript, JavaScript, Orientação a Objetos, Padrões de Projeto, Boas Práticas em Desenvolvimento de software, testes unitários).
- Experiência: bullets reordenados (arquitetura, APIs e CI/CD primeiro). "Custos" foi incluído nas decisões técnicas, porque o destaque sustenta isso.
- Competências: Front-end ganhou Blazor; Back-end ganhou C#, .NET Core e Microsserviços; a categoria virou "Arquitetura e Boas Práticas" com Orientação a Objetos e Padrões de Projeto; DevOps ganhou GitHub; Oracle e Hibernate subiram na ordem.
- Projetos: Plataforma Cortex antes de Infraspeak.

## O que falta (❌)
### Obrigatórios
- **Jenkins / Sonar**: a vaga cita CI/CD com essas ferramentas. Você tem CI/CD no Azure DevOps, o que conta bem na entrevista. Sugestão: subir um SonarQube em Docker num projeto pessoal (1 tarde) e conectar a um pipeline.
- **NodeJs**: comum em quem faz Angular. Se usou, adicione ao perfil-extra.
- **automação Python**: a Mirante vende "IA Agêntica", então scripts de automação ou integração com LLMs valem muito.
- **JSF / Desenvolvimento JEE**: stack Java legado. Você tem Java, Spring Boot, JPA e Hibernate.
- **SOAP / GraphQl**: SOAP é comum em integrações com governo e sistemas legados.
- **NoSQL**: MongoDB, Redis, Cosmos DB etc.

- **Jira / ServiceNow**: ferramentas de gestão; fáceis de citar se você já usou.

## Atenção
- **Microsserviços** é um equivalente fraco. Na entrevista, explique com a sua história real: você conhece arquitetura distribuída/mensageria (Service Bus) e soube **consolidar** sistemas quando o custo não se justificava. Isso é maturidade arquitetural e um ótimo argumento.
- **Destaque de R$ 6 mil**: prepare-se para detalhar quantos sistemas e bancos eram antes e depois, em que cloud estavam e o que mudou no DevOps. Confirmei como Grupo Orion (cargo atual). Se foi em outro lugar, me avise.
- **Senioridade**: a vaga pede Pleno (2–4 anos). Você tem cerca de 3 anos contando o estágio, e o cargo atual é de coordenação. Está dentro.
- O texto da vaga cita "Cs" na lista de tecnologias. Parece erro de digitação (CSS?) e foi ignorado.
- **Páginas**: compilado no Docker em **2 páginas** (`\titlespacing` ajustado para 18pt/9pt para preencher a 2ª página).

## Perguntas para você
- Já usou **Bootstrap**, **Node.js**, **Python** (scripts/automação) ou construiu algo com **agentes de IA**? O próprio adaptador de currículo usa Claude Code. Se for um projeto seu no GitHub, pode virar projeto no currículo.
- Usou **JPA** (Spring Data JPA) com o Spring Boot? E **SOAP** em alguma integração na Infraero?
- Já trabalhou com algum **NoSQL** (Redis, MongoDB, Cosmos DB)?
- Usou **Jira**, **SonarQube/SonarCloud** ou **Jenkins** em algum momento?
- Quantos sistemas/bancos foram consolidados no projeto de economia? Um número a mais ("5 sistemas → 1 API") deixa o destaque ainda mais forte.

Se tiver qualquer um desses, adicione ao `perfil-extra.md` e rode de novo.

## Atualização (2ª rodada)
- Entraram: **Bootstrap** (Front-end e resumo), **UML** (Arquitetura e resumo), **JPA (Spring Data JPA)** (ORM e resumo) e **Agentes IA (Claude, ChatGPT)** (Ferramentas e resumo).
- **Atenção, Agentes IA**: na vaga isso provavelmente significa *desenvolver* agentes/automação com IA. Você *usa* agentes no desenvolvimento, e o currículo diz exatamente isso. Se perguntarem, cite o adaptador de currículo, que orquestra o Claude Code com uma skill e uma interface web em Docker. Se ele estiver no seu GitHub, vale virar projeto.
- Ainda faltam: Jenkins, Sonar, NodeJs, automação Python, JSF, JEE, SOAP, GraphQl, NoSQL, Jira, ServiceNow.

## Atualização (3ª rodada)
- Novo projeto: **Automação de Climatização -- Mané Vírgula (Partage Shopping)** (WebSocket, controle em tempo real pela aplicação e automações configuráveis pelos usuários). Registrado no `perfil-extra.md`.
- Compilado no Docker: continua em **2 páginas** (38 linhas na 2ª).
- Confirmado: Grupo Orion, integrado à Plataforma Cortex, SignalR, 16 aparelhos de ar-condicionado no mercado Mané Vírgula.
