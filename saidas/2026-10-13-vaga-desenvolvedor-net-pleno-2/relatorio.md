# Relatório — Desenvolvedor .NET Pleno (Integrações) @ empresa não identificada

## Compatibilidade

| | Antes | Depois |
|---|---|---|
| Obrigatórios (20 termos) | **40%** (8/20) | **80%** (16/20) |
| Diferenciais (35 termos) | **23%** (8/35) | **34%** (12/35) |

Os diferenciais podem subir bastante se você responder as perguntas do fim deste relatório. Vários deles são coisas que quem trabalha com ASP.NET Core e Service Bus costuma usar (JWT, retry com Polly, logs).

## Palavras-chave da vaga

### Obrigatórios
| Termo (grafia da vaga) | Antes | Depois | Observação |
|---|---|---|---|
| Desenvolvedor .NET Pleno (título) | ❌ | ✅ | O título era "Engenheiro de Software Full-Stack" |
| C# | ❌ | ✅ | Não aparecia em lugar nenhum. É implícito em ASP.NET Core |
| .NET 8 ou superior | ❌ | ❌ | **Pergunta 1** |
| 3 a 4 anos com C#/.NET | ❌ | ❌ | Você tem ~2 anos e 10 meses (contando o estágio). Veja "Atenção" |
| Programação assíncrona (async/await) | ❌ | ✅ | Equivalente: Service Bus, SignalR e EF Core exigem async/await |
| ASP.NET Core Web API | 🔁 | ✅ | Era "ASP.NET Core" |
| APIs REST | ✅ | ✅ | Agora também com "construção e consumo" |
| HTTP | ❌ | ✅ | Equivalente: APIs REST. Adicionado em Competências |
| Banco de dados relacional | ✅ | ✅ | |
| SQL | ✅ | ✅ | Estava só no cabeçalho, agora também em Competências |
| SQL Server | ✅ | ✅ | |
| PostgreSQL | ✅ | ✅ | |
| Modelagem | ✅ | ✅ | |
| Queries complexas | ❌ | ❌ | **Pergunta 2** |
| Controle de transações | ❌ | ❌ | **Pergunta 2** |
| Entity Framework Core | 🔁 | ✅ | Era "Entity Framework". Com ASP.NET Core o ORM é o EF Core |
| Git | ✅ | ✅ | |
| Pull requests | ❌ | ✅ | Equivalente: revisão de código no Azure DevOps |
| Code review | 🔁 | ✅ | Era "revisão de código". Agora aparecem as duas formas |
| Integrações entre sistemas | ✅ | ✅ | Virou o primeiro bullet e o foco do resumo |

### Diferenciais ("amplia suas chances" + "desejável")
| Termo | Antes | Depois | Observação |
|---|---|---|---|
| Mensageria | ✅ | ✅ | A vaga aceita "RabbitMQ **ou similar**", e o Azure Service Bus é similar |
| Filas | ❌ | ✅ | Service Bus usa filas |
| RabbitMQ | ❌ | ❌ | Não é a mesma coisa que Service Bus, então não foi adicionado |
| Retry / timeout / circuit breaker | ❌ | ❌ | **Pergunta 3** |
| Reprocessamento / mensagens duplicadas | ❌ | ❌ | **Pergunta 3** |
| OAuth 2.0 / OpenID Connect / JWT / Microsoft Entra ID | ❌ | ❌ | **Pergunta 4** |
| Observabilidade / log estruturado / rastreio de requisição | ❌ | ❌ | **Pergunta 5** |
| Testes automatizados | ❌ | ✅ | Equivalente: xUnit |
| Testes unitários / xUnit | ✅ | ✅ | |
| Moq | ❌ | ❌ | **Pergunta 6** |
| Azure DevOps / CI/CD | ✅ | ✅ | |
| Work items | ❌ | ❌ | **Pergunta 7** |
| OpenAPI/Swagger | 🔁 | ✅ | Era só "Swagger" (Swagger é OpenAPI) |
| Scalar | ❌ | ❌ | |
| LGPD | ❌ | ❌ | **Pergunta 8** |
| iPaaS / Digibee | ❌ | ❌ | |
| CQRS / DDD / Arquitetura em camadas | ✅ | ✅ | Agora também no resumo |
| Injeção de dependência | 🔁 | ✅ | Era só "Dependency Injection" |
| Sistemas legados | ❌ | ❌ | **Pergunta 9** |
| ERP/CRM (Protheus, Dynamics) | ❌ | ❌ | A Infraspeak é gestão de manutenção, não ERP/CRM |
| Outbox / idempotência / reconciliação / versionamento de contratos | ❌ | ❌ | **Pergunta 10** |

## O que foi alterado

- **Título**: "Engenheiro de Software Full-Stack" → "Desenvolvedor .NET Pleno". É o título exato da vaga, e Pleno é um nível que você ocupou de fato.
- **Linha do cabeçalho**: ".NET • Java • Angular • Azure • SQL • Docker" → "C# • .NET • ASP.NET Core Web API • Entity Framework Core • SQL Server • PostgreSQL • Azure DevOps". Java e Angular não aparecem na vaga.
- **Tamanho (2 páginas cheias)**: a seção Objetivo saiu, porque repetia o resumo. O resumo ficou em 2 parágrafos e a empresa voltou para a linha de baixo do cargo. Os bullets ficaram em 8, 4 e 4. Os projetos principais têm título em linha própria e as competências ganharam espaçamento. Compilado com pdflatex: 2 páginas (a página 2 tem 33 linhas de texto, um pouco abaixo da meta de ~40; confirme no Overleaf).
- **Resumo**: a primeira frase agora usa "Desenvolvedor .NET" + C# + ASP.NET Core Web API + integrações. Entraram EF Core, SQL, filas, pull requests, CQRS, arquitetura em camadas, injeção de dependência, testes automatizados e CI/CD. Saíram "Full-Stack", Java e Angular (continuam em Competências).
- **Coordenador Back-end**: os bullets foram reordenados e os de integração e APIs vieram para o topo. Entraram "C#/.NET", "ASP.NET Core Web API", "pull requests" e "CI/CD".
- **Desenvolvedor Back-end Pleno**: entraram "C#", "ASP.NET Core Web API", "bancos de dados relacionais", "testes automatizados" e "injeção de dependência".
- **Infraero**: entrou "C#", e o bullet de bancos relacionais subiu.
- **Projetos**: a Integração Infraspeak passou para o primeiro lugar.
- **Competências**: as categorias foram reordenadas pela relevância para a vaga. Foram criadas as categorias "Testes" e outra com pull requests/code review, e entraram HTTP, async/await, filas, EF Core e OpenAPI/Swagger.

## O que falta (❌)

### Obrigatórios
- **3 a 4 anos de experiência com C#/.NET**: você tem cerca de 2a10m. Isso não se resolve no currículo, mas costuma ser flexível quando o resto encaixa. Vale se candidatar mesmo assim.
- **.NET 8**: alguns ATS procuram exatamente "NET 8". Se você usa, isso pesa muito (pergunta 1).
- **Queries complexas / controle de transações**: veja a pergunta 2.

### Diferenciais
- **Resiliência (retry, timeout, circuit breaker)**: muito citada na vaga (aparece 2 vezes). Se você nunca usou, vale estudar a biblioteca **Polly** ou `Microsoft.Extensions.Http.Resilience`, que leva poucas horas.
- **Autenticação (OAuth 2.0, OIDC, JWT, Entra ID)**
- **Observabilidade (log estruturado, rastreio)**: Serilog, Application Insights, OpenTelemetry.
- **RabbitMQ, Moq, Scalar, LGPD, Digibee/iPaaS, ERP/CRM, padrões de integração (outbox, idempotência)**

## Atenção

- **Equivalências que podem ser questionadas na entrevista**: async/await, HTTP e pull requests foram adicionados porque decorrem diretamente do que você já faz. Esteja pronto pra falar deles.
- **Você é Coordenador hoje e a vaga é Pleno.** O título do cabeçalho resolve o filtro do ATS, mas na entrevista prepare uma resposta sobre por que quer voltar a um papel mais técnico, ou se a vaga tem caminho para liderança.
- A vaga é **remota**. Você pode trocar "Brasília - DF" por "Brasília - DF (disponível para remoto)", se quiser.
- O nível de inglês não é exigido nessa vaga.

## Perguntas para você

Se a resposta for "sim", anote em `perfil-extra.md` e rode de novo:

1. Os projetos no Grupo Orion usam **.NET 8 (ou 9/10)**? Qual versão?
2. Você escreve **queries SQL complexas** (joins, CTEs, procedures) e usa **transações** (`BeginTransaction`, `TransactionScope`, Unit of Work)?
3. No Service Bus você já tratou **reprocessamento / dead-letter / mensagens duplicadas**? Usa **retry, timeout ou circuit breaker** (Polly)?
4. Suas APIs usam **JWT**? Alguma integração com **OAuth 2.0 / OpenID Connect / Azure AD (Entra ID)**? O SICOA (controle de acesso) usava algum desses?
5. Usa **log estruturado** (Serilog, NLog), **Application Insights**, **OpenTelemetry** ou correlation ID para investigar erros?
6. Usa **Moq** (ou NSubstitute) nos testes com xUnit?
7. Usa **work items / boards** no Azure DevOps?
8. Já aplicou algo de **LGPD** (mascaramento, anonimização, controle de dados sensíveis)?
9. Os sistemas da Infraero eram **legados**? Há sistemas legados no Grupo Orion?
10. Na integração Infraspeak você usou **idempotência**, **reconciliação** de dados ou **versionamento** de API/contrato?
11. Quantas pessoas tem a equipe que você coordena? Algum número de impacto (volume de mensagens, dispositivos IoT, tempo de resposta)?
