# AGENTS.md — API de Gestão de Treinamentos Corporativos & Compliance ESG (.NET 8 + Oracle)

## 1. Visão Geral e Contexto de Negócio (ESG)
Você deve construir uma API RESTful robusta em **.NET 8 (ASP.NET Core Web API)** para gestão de treinamentos obrigatórios, vencimentos e conformidade (compliance) de colaboradores, alinhada aos pilares de **ESG (Social e Governança)** — como Normas Regulamentadoras (NRs), Segurança do Trabalho, Ética/Anticorrupção, LGPD e Educação Ambiental.

**Premissas arquiteturais obrigatórias:**
- **Arquitetura MVC para Web API (confirmada com o professor):** A solução será exclusivamente uma API RESTful, sem interface gráfica e sem MVVM. Aplicar separação de responsabilidades compatível com MVC no ASP.NET Core: **Models** representam o domínio; **Controllers** (`[ApiController]`) recebem requisições e retornam respostas HTTP; **DTOs (Request/Response)** definem contratos de API; **Services** concentram regras de negócio; **EF Core/Data** cuida da persistência. Como não existe frontend, não criar Views Razor fictícias nem alegar que DTOs são Views. Documentar no README a adaptação do MVC para Web API e a orientação do professor.
- **Foco no Domínio Real:** Não gaste esforço construindo CRUD genérico de manutenção de usuários. O foco deve estar na matriz de obrigatoriedade por cargo, controle de validade de certificações e apuração de pendências/vencimentos.

---

## 2. Stack Tecnológica e Pacotes NuGet
- **Runtime/Framework:** .NET 8 (`net8.0`), C# 12
- **Web API & Docs:** `Microsoft.AspNetCore.OpenApi`, `Swashbuckle.AspNetCore` (com suporte a Bearer JWT no Swagger UI)
- **Persistência & ORM (Oracle):** `Microsoft.EntityFrameworkCore`, `Oracle.EntityFrameworkCore` (versão compatível com .NET 8, ex: `8.21.140` ou `8.23.*`), `Microsoft.EntityFrameworkCore.Tools`
- **Validação:** `FluentValidation.AspNetCore`
- **Segurança:** `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Containerização:** Docker (`Dockerfile` multi-stage + `.dockerignore`)
- **Testes Automatizados:** `xUnit`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory`

---

## 3. Estrutura da Solution e Entregáveis
Crie uma solution limpa organizando código-fonte, testes, Dockerfile e a collection de API:

```text
GestaoTreinamentosESG/
├── GestaoTreinamentosESG.sln
├── Dockerfile                    # Build multi-stage para ASP.NET Core 8 (porta 8080)
├── README.md                     # Arquitetura MVC para API, configuração e execução
├── .dockerignore                 # Ignora bin/, obj/, .git/, .vs/
├── collections/
│   └── GestaoTreinamentosESG.postman_collection.json  # Collection v2.1 (compatível com Postman e Insomnia)
├── src/
│   └── GestaoTreinamentosESG.Api/
│       ├── Controllers/          # Endpoints RESTful (Finos, delegando regras aos Services)
│       ├── Services/             # Regras de negócio, cálculos de vencimento e compliance
│       ├── Models/               # Entidades de domínio
│       ├── Enums/                # Classificações fixas do sistema
│       ├── DTOs/                 # Contratos de entrada/saída e envelope de paginação
│       ├── Data/
│       │   ├── Migrations/       # Arquivos de migração Oracle do EF Core (obrigatório manter no projeto)
│       │   ├── AppDbContext.cs
│       │   └── DbSeeder.cs
│       ├── Validators/           # Validadores FluentValidation
│       ├── Exceptions/           # Exceções customizadas e GlobalExceptionHandler (IExceptionHandler)
│       ├── Security/             # Geração/configuração de tokens JWT
│       ├── Program.cs
│       ├── appsettings.json      # Sem segredos reais
│       └── appsettings.Example.json
└── tests/
    └── GestaoTreinamentosESG.Tests/
        ├── CustomWebApplicationFactory.cs
        ├── Controllers/          # Testes HTTP 200 por controller, incluindo Auth
        └── Services/             # Testes unitários das regras de negócio
```

---

## 4. Modelagem de Dados e Regras de Negócio

### Política de Identificadores, Enums e Dados Iniciais
- **Colaborador é uma entidade cadastrável:** `Colaborador.Id` deve ser `Guid` (UUID), gerado pela aplicação com `Guid.NewGuid()` no cadastro, não informado pelo cliente; persistir no Oracle com mapeamento consistente do provedor (preferir `RAW(16)` com conversão testada de ponta a ponta, ou a estratégia oficialmente suportada pelo provedor escolhido). Não usar `int` nem geração sequencial para esse ID.
- **Propagar o GUID sem exceções:** `RegistroTreinamento.ColaboradorId`, parâmetros de rotas (`{colaboradorId:guid}`), filtros, contratos Request/Response, chaves estrangeiras, consultas, validações, testes e collection Postman devem usar `Guid` para identificar colaboradores.
- **Enums exclusivamente para valores fixos e fechados:** `PilarEsg` (`Ambiental`, `Social`, `Governanca`), `StatusCompliance` (`EmDia`, `ProximoVencimento`, `Vencido`, `DentroDoPrazo`, `Pendente`), `Departamento` e `TipoFuncao`/`FuncaoCodigo` (funções predefinidas), além de `PerfilAcesso` (`Admin`, `Auditor`) caso conveniente. Declarar valores numéricos explícitos e estáveis; documentar nomes e significados. Os enums vivem no código C# e **não são entidades EF Core nem geram tabelas por si mesmos**. Para respostas e requisições, serializar enum de maneira legível (strings) e validar valores desconhecidos.
- **Dados de referência fixos devem ser inseridos via seed idempotente:** `Funcao` permanece entidade/tabela relacional para suportar a matriz N:N e demais metadados (`Nome`, `Descricao`), mas seus códigos/IDs fixos são associados a `TipoFuncao` e populados exclusivamente pelo `DbSeeder`. Não criar endpoint para cadastrar/editar funções ou departamentos; `Departamento` será um enum armazenado na entidade `Funcao` (não string livre). O seed deve atualizar/completar registros de referência faltantes sem duplicá-los, inclusive em banco parcialmente populado.
- **Não transformar entidades operacionais em enum:** `Colaborador`, `Treinamento`, `RegistroTreinamento` e `FuncaoTreinamento` exigem armazenamento relacional e são dados variáveis. `Treinamento` continua cadastrável por `POST /api/treinamentos`; os exemplos iniciais de treinamentos são criados via seed apenas para demonstração, não como lista fechada de enum. Vínculos da matriz e registros de conclusão são criados pelos endpoints da API; o seed pode inserir exemplos sem duplicações.
- **Cadastro pelo usuário da API:** criar `ColaboradoresController` com `POST /api/colaboradores` (Admin, retorna 201 e o GUID gerado) e `GET /api/colaboradores` (paginado, autenticado, retorna 200), além de `GET /api/colaboradores/{colaboradorId:guid}` (autenticado). O request recebe um `TipoFuncao` predefinido/ID da função existente, nunca texto arbitrário; validar unicidade de matrícula/email. O seed pode conter colaboradores fictícios para testes/demonstração, mas novos colaboradores devem ser cadastráveis via endpoint.

### Entidades Principais
1. **`Funcao` (catálogo fixo de Cargo/Papel, via seed):**
   - `Id` (int estável, valor correspondente a `TipoFuncao`), `Nome` (string, max 100), `Departamento` (enum fixo), `Descricao` (string, max 300). Não permitir criação de novos tipos de função pela API nesta entrega.
2. **`Colaborador` (cadastrável via API):**
   - `Id` (**Guid**, chave primária), `Nome` (string, max 150), `Matricula` (string, max 30, única), `Email` (string, max 150, único), `DataAdmissao` (DateTime UTC), `Ativo` (bool), `FuncaoId` (FK int para `Funcao` proveniente de seed).
3. **`Treinamento` (cadastrável via API):**
   - `Id` (int), `Titulo` (string, max 150), `Descricao` (string, max 500), `PilarEsg` (enum: `Ambiental`, `Social`, `Governanca`), `CargaHorariaHoras` (int), `ValidadeMeses` (int; `0` significa sem vencimento).
4. **`FuncaoTreinamento` (Matriz de Obrigatoriedade N:N, alterável por endpoint):**
   - `FuncaoId` (int), `TreinamentoId` (int), `Obrigatorio` (bool, padrão `true`), `PrazoDiasAposAdmissao` (int). Criar chave composta e impedir associações duplicadas.
5. **`RegistroTreinamento` (Histórico de Realizações, via API):**
   - `Id` (int), `ColaboradorId` (**Guid**, FK para `Colaborador`), `TreinamentoId` (int), `DataConclusao` (DateTime UTC), `DataValidade` (DateTime? calculada), `NotaAvaliacao` (decimal? com `.HasPrecision(5, 2)`), `CertificadoCodigo` (string, max 60).

### Regras de Negócio Essenciais
- **Cálculo Automático de Validade:** Ao registrar a conclusão de um treinamento, se `Treinamento.ValidadeMeses > 0`, calcular `DataValidade = DataConclusao.AddMonths(Treinamento.ValidadeMeses)`. Caso contrário, `DataValidade = null` (vitalício).
- **Status de Compliance do Colaborador:** Para cada treinamento obrigatório vinculado à `Funcao` do colaborador, considerar sempre o **registro mais recente** realizado por ele e classificar em:
  - `EmDia`: Possui registro válido (`DataValidade == null` ou `DataValidade > DateTime.UtcNow.AddDays(30)`).
  - `ProximoVencimento`: Possui registro válido, mas vence nos próximos 30 dias.
  - `Vencido`: O registro mais recente está com `DataValidade <= DateTime.UtcNow`.
  - `DentroDoPrazo`: Nunca realizou, mas ainda está dentro do prazo definido em `DataAdmissao.AddDays(PrazoDiasAposAdmissao)` (não é inadimplência).
  - `Pendente`: Nunca realizou e já ultrapassou o prazo após admissão (ou o prazo é zero e a obrigação já é exigível).
- **Critérios de corte:** Usar horário UTC e um relógio injetável (`TimeProvider`) para testes determinísticos. `Vencido` quando `DataValidade <= agora`; `ProximoVencimento` quando `agora < DataValidade <= agora.AddDays(30)`; `EmDia` quando não vence ou vence após esse limite. Se houver registro anterior vencido, classificar como `Vencido` (não como `Pendente`).
- **Percentual de aderência:** Definir no README e implementar consistentemente: número de obrigações `EmDia` ou `ProximoVencimento` dividido pelo total de obrigações já exigíveis (`EmDia`, `ProximoVencimento`, `Vencido`, `Pendente`) multiplicado por 100; excluir `DentroDoPrazo` do denominador. Caso não haja obrigações exigíveis, retornar 100% e total igual a zero, documentando a convenção. Na listagem de pendências, não incluir `DentroDoPrazo` por padrão.
- **Seed Inicial (`DbSeeder`):** Garantir individualmente (por chave estável/índice único) a existência dos dados fixos de referência e dos exemplos de demonstração, sem depender exclusivamente de `if (!await context.Funcoes.AnyAsync())`. Popular 3 funções fixas associadas a `TipoFuncao`, 4 treinamentos ESG demonstrativos, matriz de obrigatoriedade, 5 colaboradores fictícios com **GUIDs determinísticos conhecidos** para o seed ser repetível, e registros em diferentes estados: em dia, vencido, a vencer, pendente e, quando possível, dentro do prazo. Para os novos colaboradores cadastrados pela API, usar `Guid.NewGuid()`. Não sobrescrever dados operacionais existentes no reseed.

---

## 5. Requisitos Técnicos Obrigatórios

### A. Banco de Dados Oracle (FIAP) e Migrations
- Configurar `AppDbContext` usando `options.UseOracle(connectionString)` do pacote `Oracle.EntityFrameworkCore`.
- Definir tamanhos máximos explícitos (`HasMaxLength`) nas propriedades `string` e `.HasPrecision(5, 2)` em `NotaAvaliacao` para gerar colunas `VARCHAR2` e `NUMBER(5,2)` otimizadas no Oracle. Mapear `Guid` de colaborador e FK com a mesma representação Oracle e conversões verificadas; garantir que migrations e testes reflitam esses tipos. Persistir enums usando conversões EF Core explícitas e consistentes.
- Documentar em `appsettings.Example.json` uma string de conexão **apenas com placeholders** no formato do Oracle da FIAP (não substituir por credenciais reais na entrega):
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=oracle.fiap.com.br:1521/ORCL;User Id=SEU_RM;Password=SUA_SENHA;"
  }
  ```
- **Obrigatório:** Gerar a migration inicial (`dotnet ef migrations add InitialCreate`) direcionada para o provedor Oracle e garantir que todos os arquivos gerados em `src/GestaoTreinamentosESG.Api/Data/Migrations/` permaneçam no projeto.
- Na inicialização da API (`Program.cs`, quando não estiver no ambiente `Testing`), executar `await dbContext.Database.MigrateAsync()` seguido de `await DbSeeder.SeedAsync(dbContext)` quando houver conexão configurada. Se estiver ausente, registrar diagnóstico objetivo e não declarar que o banco foi inicializado. Não engolir indiscriminadamente falhas de migration ou de conexão; sinalizar falha de inicialização de forma verificável. Documentar também `dotnet ef database update`.

### B. Paginação e Otimização de Consultas
- Criar estrutura genérica reutilizável:
  - `PaginationParams` (`PageNumber = 1`, `PageSize = 10`, com limite máximo de `50` itens por página).
  - `PagedResponse<T>` contendo `Items`, `PageNumber`, `PageSize`, `TotalCount` e `TotalPages`.
- **Otimização obrigatória:** Todas as consultas de leitura/listagem devem utilizar `.AsNoTracking()`, projeção direta para DTOs (evitando problemas de N+1) e ordenação determinística (`OrderBy` com critério de desempate único) antes de `Skip`/`Take`. Criar índices para FKs e filtros utilizados com frequência; executar `CountAsync` e projeções paginadas no banco.

### C. Endpoints RESTful (Mínimo de 4 fluxos de alto valor)
Implementar Controllers bem estruturados com verbos HTTP adequados, códigos de status semânticos (`200`, `201`, `204`, `400`, `401`, `403`, `404`, `422`) e documentação Swagger habilitada:

1. **`AuthController` (`/api/auth`):**
   - `POST /api/auth/login` — Valida credenciais de demonstração configuradas em `appsettings.json` (`admin@esg.com` / `Admin@123` com role `Admin` e `auditor@esg.com` / `Auditor@123` com role `Auditor`) e emite um **JWT Token** válido.
2. **`ColaboradoresController` (`/api/colaboradores`):**
   - `POST /api/colaboradores` — Cadastra colaborador (restrito a `Admin`), gera `Guid` no servidor e retorna `201 Created`, com `Location` para consulta do novo recurso.
   - `GET /api/colaboradores` — Lista colaboradores com paginação e filtros opcionais por `funcaoId` e `ativo` (requer autenticação).
   - `GET /api/colaboradores/{colaboradorId:guid}` — Consulta colaborador pelo GUID (requer autenticação; retorna 404 se inexistente).
3. **`TreinamentosController` (`/api/treinamentos`):**
   - `GET /api/treinamentos` — Listagem paginada com filtro opcional por `pilarEsg` e busca por título.
   - `POST /api/treinamentos` — Cadastra novo treinamento ESG (restrito a `Admin`).
4. **`MatrizObrigatoriedadeController` (`/api/funcoes`):**
   - `GET /api/funcoes` — Lista funções com os treinamentos obrigatórios associados (paginado).
   - `POST /api/funcoes/{funcaoId}/treinamentos/{treinamentoId}` — Vincula um treinamento como obrigatório para uma função (restrito a `Admin`).
5. **`RegistrosTreinamentoController` (`/api/registros-treinamento`):**
   - `POST /api/registros-treinamento` — Registra a conclusão de um treinamento por um colaborador identificado por GUID, validando existência de ambos e calculando `DataValidade` automaticamente (restrito a `Admin`).
   - `GET /api/registros-treinamento` — Histórico paginado de treinamentos realizados com filtros por `colaboradorId` (**Guid**) ou `treinamentoId` (int).
6. **`ComplianceController` (`/api/compliance`):**
   - `GET /api/compliance/colaboradores/{colaboradorId:guid}` — Retorna o dossiê completo de conformidade do colaborador (função, lista de treinamentos obrigatórios, status individual de cada um e percentual geral de aderência).
   - `GET /api/compliance/pendencias` — Endpoint paginado que lista todos os colaboradores ativos com treinamentos `Vencido`, `Pendente` ou `ProximoVencimento`, filtrável por `status` e `departamento`.
   - `GET /api/compliance/indicadores-esg` — Resumo consolidado da taxa de conformidade da empresa agrupada por `PilarEsg` (`Ambiental`, `Social`, `Governanca`).

### D. Segurança, Validação e Tratamento Global de Exceções
- **Autenticação & Autorização:** Configurar JWT Bearer (`AddAuthentication().AddJwtBearer()`). Proteger endpoints de escrita (`POST`, `PUT`, `DELETE`) com `[Authorize(Roles = "Admin")]` e endpoints sensíveis de consulta/compliance com `[Authorize]`.
- **Validação:** Validar payloads de entrada com FluentValidation (ex: `ValidadeMeses >= 0`, `DataConclusao <= DateTime.UtcNow`, `CargaHorariaHoras > 0`, `NotaAvaliacao` entre 0 e 10 e `PrazoDiasAposAdmissao >= 0`, `ColaboradorId != Guid.Empty`, `FuncaoId` precisa existir no catálogo fixo e `Matricula`/`Email` precisam ser únicos). Rejeitar datas de conclusão futuras e entradas de paginação inválidas.
- **Tratamento Global de Exceções:** Implementar `IExceptionHandler` (.NET 8) mapeando exceções de domínio (`NotFoundException` -> 404, `BusinessRuleException` -> 422, erros inesperados -> 500) para o padrão RFC 7807 (`ProblemDetails`).

### E. Docker (`Dockerfile` e `.dockerignore`)
- Criar um `Dockerfile` multi-stage na raiz do repositório para empacotar a API .NET 8 que se conectará ao Oracle externo:
  - Stage `build` usando `mcr.microsoft.com/dotnet/sdk:8.0` para restaurar, compilar e publicar `src/GestaoTreinamentosESG.Api/GestaoTreinamentosESG.Api.csproj`.
  - Stage `final` usando `mcr.microsoft.com/dotnet/aspnet:8.0`, expondo a porta `8080`, definindo `ASPNETCORE_URLS=http://+:8080` e `ASPNETCORE_ENVIRONMENT=Development` **apenas no cenário de demonstração local** (mantendo Swagger em `/swagger`); não fixar Development como padrão obrigatório de produção.
- Criar `.dockerignore` excluindo `**/bin`, `**/obj`, `.git`, `.vs`.

### F. Collection Postman / Insomnia (`collections/GestaoTreinamentosESG.postman_collection.json`)
- Gerar na pasta `collections/` um arquivo JSON válido no formato **Postman Collection v2.1** (importado nativamente tanto pelo **Postman** quanto pelo **Insomnia**).
- Deve conter:
  - Variáveis de coleção: `base_url` (`http://localhost:8080`) e `jwt_token` (vazia inicialmente).
  - Autenticação Bearer configurada no nível da coleção usando `{{jwt_token}}`.
  - Pasta **1. Auth** com `POST {{base_url}}/api/auth/login` (`admin@esg.com` / `Admin@123`) e script de pós-resposta (`Tests`) que extrai automaticamente o token e salva em `pm.collectionVariables.set("jwt_token", jsonData.token)`.
  - Pastas para **2. Colaboradores**, **3. Treinamentos**, **4. Matriz de Obrigatoriedade**, **5. Registros de Treinamento** e **6. Compliance & Indicadores ESG**, cobrindo todos os endpoints `GET` e `POST` com parâmetros de paginação e bodies JSON válidos prontos para execução. Incluir variável `colaborador_id` com GUID de exemplo do seed; no script pós-resposta de `POST /api/colaboradores`, salvar o novo GUID em `colaborador_id`, que será utilizado nos requests de registros e compliance. Documentar enums aceitos e garantir que a collection use o mesmo formato de serialização da API.

### G. Testes Automatizados com xUnit
- No projeto `GestaoTreinamentosESG.Tests`, configurar `CustomWebApplicationFactory<Program>` utilizando `UseEnvironment("Testing")` e banco isolado de teste `UseInMemoryDatabase` (nome único por execução para evitar contaminação). Remover os registros anteriores de `DbContextOptions<AppDbContext>` e do provedor Oracle antes de registrar InMemory, evitando conflito de providers; garantir que não rodem migrations Oracle no ambiente `Testing`.
- **Regra obrigatória:** Cada Controller (`AuthController`, `ColaboradoresController`, `TreinamentosController`, `MatrizObrigatoriedadeController`, `RegistrosTreinamentoController`, `ComplianceController`) deve ter **no mínimo um teste xUnit** que confirme explicitamente **HTTP 200 OK** em uma rota apropriada. Para `AuthController`, usar `POST /api/auth/login` com credenciais de demonstração válidas; para os demais, uma rota `GET` que retorne 200.
- Estes testes HTTP com `WebApplicationFactory` são **testes de integração**, embora usem xUnit. Complementar com testes **unitários de Services** para cálculo de `DataValidade`, limites dos status de compliance, prazo após admissão e percentual de aderência.
- Nos testes HTTP, autenticar previamente quando necessário, enviando JWT válido ou configurando uma autenticação de teste bem delimitada; popular o banco de teste com dados conhecidos.
- Testar 400, 401, 403, 404 e 422 quando aplicável e garantir que exceções não vazem informações sensíveis.
- **Usar asserção exata de status:** `EnsureSuccessStatusCode()` verifica apenas 2xx; não é suficiente para comprovar especificamente HTTP 200. Exemplo:

```csharp
[Fact]
public async Task Get_ReturnsHttpStatusCode200()
{
    // Arrange (cliente autenticado quando necessário)
    var request = "/api/treinamentos";

    // Act
    var response = await _client.GetAsync(request);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

Adicionar `using System.Net;` e `using Xunit;` nos arquivos que precisarem.

---

## 6. Ordem de Execução Obrigatória para o Codex (incremental)
Não gerar toda a aplicação de uma vez sem validação. Trabalhar por etapas, corrigindo falhas antes de avançar. Ao final de cada etapa, descrever arquivos modificados, comandos executados, resultados e pendências verificáveis.

1. **Estrutura:** Criar solution e projetos (`Api`, `Tests`), configurar pacotes em versões compatíveis com .NET 8; executar `dotnet restore` e `dotnet build`.
2. **Domínio e banco:** Implementar Models, mapeamento EF Core, índices, `AppDbContext` e migration inicial **real para Oracle**, incluindo os arquivos gerados; executar build. Não inventar migration manuscrita se o comando falhar: relatar erro e corrigir.
3. **Base transversal:** Implementar configuração por ambiente, tratamento global RFC 7807, JWT, autorização, Swagger e DbSeeder idempotente; executar build e testes disponíveis.
4. **Fluxos de negócio:** Implementar DTOs, validadores, Services e Controllers por fluxo (colaboradores → treinamentos → matriz → registros → compliance), incluindo paginação e consultas otimizadas; executar build e testes a cada fluxo.
5. **Testes:** Cobrir os seis Controllers com HTTP 200, cenários de segurança e regras de Services; executar `dotnet test` e corrigir falhas antes de prosseguir.
6. **Infraestrutura e documentação:** Criar Dockerfile, `.dockerignore`, README e Postman Collection v2.1; validar JSON e executar `docker build` quando o Docker estiver disponível.
7. **Verificação final:** Executar `dotnet restore`, `dotnet build` e `dotnet test`; verificar migrations, collection, endpoints e estrutura dos ZIPs. Não declarar sucesso em integração Oracle, Docker ou importação Postman/Insomnia sem ter executado essas verificações. Se depender de credenciais ou ferramentas indisponíveis, marcar como **não verificado** e fornecer passos para validação manual.

---

## 7. Checklist Final de Entrega (FIAP)
1. **Arquitetura:** README explica MVC aplicado à Web API, separando Controllers, Models, DTOs, Services e persistência, **sem criar frontend nem Views artificiais**.
2. **Credenciais Oracle:** Manter valores reais fora do projeto/ZIP. Configurar conexão por variável de ambiente `ConnectionStrings__DefaultConnection` ou .NET user-secrets; manter apenas placeholders em exemplos.
3. **Banco:** Confirmar que migrations Oracle foram geradas e versionadas. Quando houver acesso ao servidor, rodar `dotnet ef database update` ou inicialização configurada e confirmar tabelas/seed. Sem acesso, reportar validação pendente.
4. **Testes e build:** `dotnet restore`, `dotnet build` e `dotnet test` com resultados registrados; seis Controllers cobertos com HTTP 200 exato e regras de negócio testadas.
5. **Docker:** Incluir Dockerfile e `.dockerignore`, validar `docker build` se possível e documentar conexão ao Oracle externo.
6. **Collection:** JSON Postman v2.1 válido, cobrindo login e todos os endpoints. Importar no Postman ou Insomnia e executar a sequência quando o ambiente estiver disponível; distinguir validação JSON de teste real de importação.
7. **Geração dos `.ZIP` separados:**
   - **`.ZIP 1` (Código-fonte):** Solution completa com README, fontes, testes, Dockerfile, `.dockerignore` e migrations Oracle. Excluir `bin/`, `obj/`, `.git/`, `.vs/`, segredos e arquivos locais sensíveis.
   - **`.ZIP 2` (Collection):** Compactar `collections/GestaoTreinamentosESG.postman_collection.json`, contendo exemplos e variáveis sem segredos.
8. **Auditoria de entrega:** Listar o conteúdo dos dois ZIPs e conferir que não estão vazios nem faltando migrations, Dockerfile, projetos ou collection. Não considerar a entrega finalizada com verificações obrigatórias falhando.

---

## 8. Critérios de Aceitação
- **Valor de negócio:** Os quatro fluxos centrais devem funcionar: catálogo ESG, matriz de obrigatoriedade, registro com cálculo de validade e apuração de compliance/indicadores; cadastro de colaboradores por GUID serve de suporte aos fluxos.
- **HTTP e segurança:** Rotas, DTOs, validação, códigos HTTP e regras de autorização coerentes. Endpoints de escrita restritos a Admin; consultas sensíveis exigem autenticação.
- **Desempenho:** Toda listagem paginada deve ordenar no banco e usar `Skip`/`Take`; evitar N+1 e rastreamento desnecessário.
- **Regras verificáveis:** Cobrir limites de vencimento, vitalícios, histórico mais recente, prazos de admissão, percentual de aderência, geração de GUID no cadastro, mapeamento da FK GUID e seed idempotente de catálogos fixos.
- **Qualidade:** Projeto compila, testes executam e não há alegações de validação que não tenham sido de fato realizadas.
- **Entrega acadêmica:** Dois ZIPs independentes, com arquivos corretos, sem dados pessoais de acesso, e documentação suficiente para o professor executar a API.
