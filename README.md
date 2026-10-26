# API de Locação de Imóveis

API REST para cadastro de clientes e imóveis e gerenciamento de reservas. Desenvolvida em .NET 10 com ASP.NET Core, Entity Framework Core e SQLite em memória, a solução separa domínio, casos de uso, persistência e interface HTTP em quatro projetos.

## Funcionalidades

- **Clientes:** cadastro, consulta por ID ou CPF, filtros, paginação, atualização e ativação/inativação.
- **Imóveis:** cadastro, consulta, filtros, paginação, atualização, alteração de status e disponibilidade por período.
- **Reservas:** criação, consulta, filtros, paginação, confirmação, conclusão e cancelamento com cálculo de reembolso.
- **API:** documentação OpenAPI/Swagger, erros em formato `ProblemDetails` e endpoint `/health`.

## Executar localmente

Requer o SDK do **.NET 10**. Na raiz do repositório:

```powershell
dotnet restore LocacaoImoveis.slnx
dotnet build LocacaoImoveis.slnx
dotnet run --project src/LocacaoImoveis.API/LocacaoImoveis.API.csproj --launch-profile http
```

Com a API em execução no perfil `http`:

| Recurso | Endereço |
|---|---|
| Swagger UI | <http://localhost:5088/swagger> |
| Especificação OpenAPI | <http://localhost:5088/openapi/v1.json> |
| Health check | <http://localhost:5088/health> |

Swagger e OpenAPI são disponibilizados no ambiente `Development`, configurado pelo perfil acima. Os modelos, exemplos e códigos de resposta podem ser consultados no Swagger.

### Dados iniciais

Na inicialização, `EnsureCreated()` cria as tabelas e o seed insere três clientes, três imóveis e duas reservas. A conexão SQLite com `Data Source=:memory:` permanece aberta enquanto a aplicação está em execução. **Os dados são descartados ao encerrar o processo** e recriados na próxima inicialização.

## Estrutura da solução

| Projeto | Responsabilidade |
|---|---|
| `src/LocacaoImoveis.Domain` | Entidades, objetos de valor, enums e políticas de desconto e reembolso |
| `src/LocacaoImoveis.Application` | Serviços, DTOs, contratos de repositório e exceções |
| `src/LocacaoImoveis.Infrastructure` | `AppDbContext`, repositórios e carga inicial de dados |
| `src/LocacaoImoveis.API` | Controllers, middleware, injeção de dependência e OpenAPI |
| `tests/LocacaoImoveis.Domain.Tests` | Testes das entidades e políticas |
| `tests/LocacaoImoveis.Integration.Tests` | Testes de persistência, serviços e endpoints HTTP |

O `AppDbContext` mapeia CPF e e-mail como objetos de valor convertidos para texto, cria índices únicos para ambos e configura as chaves estrangeiras de reservas para clientes e imóveis.

## API

As rotas de clientes, imóveis e reservas usam o prefixo `/api/v1`.

| Recurso | Rotas principais |
|---|---|
| Clientes | `POST /api/v1/clientes/criarCliente` · `GET /api/v1/clientes/obterTodosClientes` · `GET /api/v1/clientes/{id}/detalhes` |
| Imóveis | `POST /api/v1/imoveis` · `GET /api/v1/imoveis/listar` · `GET /api/v1/imoveis/imoveisDisponiveis` |
| Reservas | `POST /api/v1/reservas` · `GET /api/v1/reservas` · `GET /api/v1/reservas/{id}/detalhes` · `PUT /api/v1/reservas/{id}/cancelar` |

As demais operações, os parâmetros e os corpos das requisições estão no [guia da API e casos de uso](docs/casos-de-uso.md).

### Filtros e paginação

As listagens usam `Pagina=1` e `TamanhoPagina=10` por padrão; o tamanho permitido vai de 1 a 100. Os filtros opcionais são aplicados antes da paginação no banco.

```text
GET /api/v1/clientes/obterTodosClientes?Nome=pedro&Status=1&Pagina=1&TamanhoPagina=10
GET /api/v1/reservas?Status=0&Pagina=1&TamanhoPagina=10
```

Clientes aceitam nome e status; reservas aceitam cliente, imóvel e status; imóveis aceitam status, capacidade e faixa de preço. As respostas são listas, sem total de registros ou páginas. Uma página sem resultados retorna `[]`.

### Regras de reserva

Uma reserva exige cliente ativo, imóvel disponível, capacidade suficiente e período sem conflito com reservas pendentes ou confirmadas. A estadia deve ter de 1 a 30 noites; um cliente pode manter até três reservas ativas. Reservas novas começam como `Pendente` e seguem as transições `Pendente → Confirmada → Concluída` ou `Pendente/Confirmada → Cancelada`.

O valor é calculado a partir das diárias. O desconto por duração é configurado em `src/LocacaoImoveis.API/appsettings.json`; a configuração atual aplica **10% a partir de sete noites**. No cancelamento, a política calcula e persiste o reembolso. O endpoint retorna `204`; o valor pode ser consultado nos detalhes da reserva.

## Testes

```powershell
dotnet test LocacaoImoveis.slnx
```

Os testes incluem regras de domínio, persistência e chamadas HTTP com `WebApplicationFactory`, inclusive tentativas concorrentes de reservar o mesmo imóvel e período.

## Documentação

- [Arquitetura e classes](docs/classes-e-arquitetura.md)
- [Casos de uso e exemplos HTTP](docs/casos-de-uso.md)
- [Fluxos de execução](docs/sequencias.md)
- [Galeria UML](docs/uml/galeria.html) — fontes PlantUML em `docs/uml/fontes` e imagens em `docs/uml/imagens`
- [Uso de inteligência artificial](docs/AI_USAGE.md)
