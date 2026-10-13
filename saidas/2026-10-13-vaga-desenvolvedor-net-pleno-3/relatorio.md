# Relatório — Desenvolvedor .NET Pleno @ vaga

## Compatibilidade
| | Antes | Depois |
|---|---|---|
| Obrigatórios (6 termos) | 67% (4/6) | 83% (5/6) |
| Diferenciais (12 termos) | 33% (4/12) | 50% (6/12) |

Observação: critério por termo explícito no currículo. "Async/await", "HTTP" e ".NET 8" continuam sem evidência e não foram adicionados.

## Palavras-chave da vaga
### Obrigatórios
| Termo (grafia da vaga) | Antes | Depois | Observação |
|---|---|---|---|
| C# e .NET | ✅ | ✅ | No título, cabeçalho, resumo e bullets |
| ASP.NET Core Web API | 🔁 (ASP.NET Core) | ✅ | Grafia da vaga; atuação real em APIs REST com ASP.NET Core |
| APIs REST (construir e consumir) | ✅ | ✅ | "Construção e evolução" e "integração via APIs REST" |
| Banco de dados relacional (SQL Server, PostgreSQL) | ✅ | ✅ | Presente |
| Entity Framework Core ou ORM equivalente | 🔁 (Entity Framework) | ✅ | Escrito como Entity Framework Core (usado com ASP.NET Core) |
| Git, pull requests, code review | 🔁 (Git, revisão de código) | ✅ | "Pull requests" e "Code Review" adicionados em bullet e competências |
| Programação assíncrona (async/await) | ❌ | ❌ | Sem evidência (mensageria assíncrona não equivale) |
| HTTP (métodos, status, cabeçalhos) | ❌ | ❌ | Sem evidência |
| .NET 8 ou superior | ❌ | ❌ | Versão não informada |

### Diferenciais
| Termo | Antes | Depois | Observação |
|---|---|---|---|
| CQRS | ✅ | ✅ | Em competências; agora também no resumo |
| DDD | ✅ | ✅ | Presente |
| Arquitetura em camadas | ✅ | ✅ | Presente |
| Injeção de dependência | 🔁 (Dependency Injection) | ✅ | Grafia da vaga |
| Mensageria / filas | ✅ | ✅ | Azure Service Bus |
| RabbitMQ | ❌ | ❌ | Não equivale ao Azure Service Bus |
| xUnit | ✅ | ✅ | Presente |
| Azure DevOps / pipelines CI/CD | ✅ | ✅ | Presente |
| OpenAPI/Swagger | 🔁 (Swagger) | ✅ | "OpenAPI/Swagger" |
| Sistemas legados | ❌ | ❌ | Sem evidência explícita |
| Moq, Scalar, OAuth/JWT/Entra ID, observabilidade, LGPD, iPaaS/Digibee, ERP/CRM, outbox/idempotência | ❌ | ❌ | Sem evidência |

## O que foi alterado
- Título/cabeçalho — "Engenheiro de Software Full-Stack" e tecnologias genéricas → "Desenvolvedor .NET Pleno" e C#, .NET, ASP.NET Core Web API, APIs REST, Entity Framework Core, SQL Server, PostgreSQL — espelha o título e os termos principais
- Objetivo — removido (padrão do layout)
- Resumo — 3 parágrafos → 2, focados em integrações, Web API, EF Core, SQL, filas, CQRS/DDD, injeção de dependência e xUnit
- Bullets — reordenados e reescritos para integrações, Web API, pull requests e CI/CD
- Competências — reorganizadas, com Testes, Pull Requests, Code Review, Entity Framework Core e OpenAPI/Swagger; Java/Spring Boot mantidos como Back-end secundário
- Projetos — Integração Infraspeak primeiro; SIVWEB e SICOA em 1 linha
- Idiomas — em 1 linha

## O que falta (❌)
### Obrigatórios
- Programação assíncrona (async/await) — citado explicitamente; é provável que você use no dia a dia — adicione ao perfil-extra.md se for verdade
- Fundamentos de HTTP — métodos, status codes, cabeçalhos — adicione se for verdade
- .NET 8 ou superior — versão usada nos projetos
- Queries complexas e controle de transações — a vaga cita; o currículo só diz "modelagem e manutenção"

### Diferenciais
- RabbitMQ/similar, retry, timeout, circuit breaker (resiliência)
- OAuth 2.0, OpenID Connect, JWT, Microsoft Entra ID
- Observabilidade, log estruturado, rastreio de requisições
- Moq, Scalar, LGPD
- iPaaS/Digibee, ERP/CRM, sistemas legados, padrões de integração (outbox, idempotência, reconciliação, versionamento de contratos)

## Atenção
- A vaga pede 3–4 anos com C#/.NET. Pelo currículo: estágio desde Dez/2023 (com .NET) + Abr/2025 em diante, o que dá cerca de 2,5–3 anos contando o estágio; um recrutador pode questionar
- "Pleno" está sustentado pelo cargo Desenvolvedor Back-end Pleno no Grupo Orion
- "Entity Framework Core" foi escrito com base em Entity Framework com ASP.NET Core; confirme que usou a versão Core
- Mensageria com Azure Service Bus é aceita como "filas", mas a vaga prefere RabbitMQ ou similar; esteja pronto para explicar
- A vaga é remota: o currículo mantém Brasília - DF, sem alteração de contato
- Verifique na compilação se o PDF ficou com 2 páginas cheias; se sobrar espaço, aumente o `\titlespacing`

## Perguntas para você
- Você usa async/await em produção no C#? Em qual versão do .NET (8+)? Se sim, adicione ao perfil-extra.md e rode de novo
- Você conhece os fundamentos de HTTP (métodos, status codes, cabeçalhos) por trabalhar com APIs? Se sim, adicione
- Já escreveu queries complexas ou usou transações no banco? Se sim, adicione
- Já tratou reprocessamento, mensagens duplicadas, retry ou timeout nas filas do Azure Service Bus?
- Já usou JWT/OAuth, log estruturado, Moq ou RabbitMQ, mesmo em estudo?
- Já trabalhou com sistemas legados ou ERP/CRM?


## Conferência automática

- PDF compilado: **2 página(s)**, 2ª página com 36 linhas de texto.
- Trava "não inventar": nenhuma tecnologia sem base no currículo/perfil.
