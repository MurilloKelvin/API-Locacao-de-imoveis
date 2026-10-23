# API de Locação de Imóveis

API REST para cadastro de clientes e imóveis e para criação e acompanhamento de reservas. O projeto foi desenvolvido em ASP.NET Core com separação entre domínio, aplicação, infraestrutura e API.

## O que já existe

- cadastro, consulta, atualização e ativação/inativação de clientes;
- validação de CPF e e-mail como objetos de valor;
- cadastro, consulta, filtros, atualização e alteração de status de imóveis;
- criação, consulta e transição de status de reservas;
- política de desconto por duração da reserva;
- política de reembolso no cancelamento;
- persistência com Entity Framework Core e SQLite em memória;
- carga inicial de dados para facilitar o uso no Swagger;
- documentacao dos endpoints com resumo, descricao e codigos de resposta no Swagger;
- testes unitários, de persistência, de serviço e de HTTP.

## Tecnologias

- .NET 10;
- ASP.NET Core Web API;
- Entity Framework Core com SQLite;
- Swagger/OpenAPI;
- xUnit;
- `WebApplicationFactory` nos testes HTTP.

As versões dos pacotes estão nos arquivos `.csproj`.

## Organização do código

| Projeto | Responsabilidade |
|---|---|
| `src/LocacaoImoveis.Domain` | entidades, enums, objetos de valor, validações e políticas de negócio |
| `src/LocacaoImoveis.Application` | DTOs, interfaces, serviços e exceções da aplicação |
| `src/LocacaoImoveis.Infrastructure` | `DbContext`, repositórios e seed |
| `src/LocacaoImoveis.API` | controllers, middleware, configuração e execução da API |
| `tests/LocacaoImoveis.Domain.Tests` | testes das regras de domínio |
| `tests/LocacaoImoveis.Integration.Tests` | testes de persistência, serviços e endpoints HTTP |

## Executar localmente

Pré-requisito: SDK do .NET 10 instalado.

Na raiz do repositório:

```powershell
dotnet restore LocacaoImoveis.slnx
dotnet build LocacaoImoveis.slnx
dotnet run --project src/LocacaoImoveis.API/LocacaoImoveis.API.csproj --launch-profile http
```

Com a API em execução:

- Swagger: <http://localhost:5088/swagger>
- OpenAPI: <http://localhost:5088/openapi/v1.json>
- HTTPS: <https://localhost:7216/swagger>
- Health check: <http://localhost:5088/health>

O banco usa SQLite em memória. Ele é criado e populado pelo seed quando a aplicação inicia e é perdido quando a aplicação é encerrada.

O endpoint `/health` verifica se a API esta respondendo e retorna `200` quando esta saudavel ou `503` quando falha.

## Testes

```powershell
dotnet test LocacaoImoveis.slnx
```

## Endpoints principais

Todos os endpoints usam o prefixo `/api/v1`.

| Recurso | Operações |
|---|---|
| Clientes | `POST /clientes/criarCliente`, `GET /clientes/{id}/detalhes`, `GET /clientes/{cpf}/detalhes`, `GET /clientes/obterTodosClientes`, `PUT /clientes/{id}/atualizar`, `PUT /clientes/{id}/ativar`, `PUT /clientes/{id}/inativar` |
| Imóveis | `POST /imoveis`, `GET /imoveis/{id}/detalhes`, `GET /imoveis/listar`, `GET /imoveis/imoveisDisponiveis`, `PUT /imoveis/{id}/atualizarImovel`, `PUT /imoveis/{id}/atualizarStatus` |
| Reservas | `POST /reservas`, `GET /reservas`, `GET /reservas/{id}/detalhes`, `PUT /reservas/{id}/cancelar`, `PUT /reservas/{id}/confirmar`, `PUT /reservas/{id}/concluir` |
| Health | `GET /health` |

Os modelos de request, os parâmetros de consulta e os exemplos de resposta estão disponíveis no Swagger.

## Documentacao no Swagger

Os controllers possuem metadados de documentacao para cada operacao. No Swagger, cada endpoint apresenta:

- resumo da operacao;
- descricao do comportamento;
- parametros de rota, query string e corpo;
- modelos de entrada e saida;
- exemplos de request para clientes, imoveis e reservas;
- codigos de resposta previstos.

Para consultar, execute a API e abra <http://localhost:5088/swagger>.

O OpenAPI e o arquivo que descreve os endpoints, os dados de entrada e as respostas da API. O Swagger usa esse arquivo para montar a tela de documentacao.

O projeto da API habilita `GenerateDocumentationFile` para gerar `LocacaoImoveis.API.xml` junto da DLL durante o build. Os comentários `///` dos controllers descrevem operações, parâmetros e respostas. O `AddOpenApi()` incorpora esses comentários ao documento exibido pelo Swagger. Após editar os comentários, recompile e reinicie a API.

O arquivo `OpenApiExamplesTransformer.cs` adiciona exemplos de request e response ao OpenAPI. Assim, o Swagger mostra modelos prontos para criar clientes, imoveis e reservas. Esses exemplos servem apenas como modelo e nao sao executados automaticamente.

## Filtros e paginação de clientes e reservas

As duas listagens aceitam `Pagina` (padrão 1) e `TamanhoPagina` (padrão 10, entre 1 e 100). Os filtros são opcionais e podem ser combinados. A resposta continua sendo uma lista, sem total de registros; páginas sem resultados retornam `[]`.

- Clientes: `GET /api/v1/clientes/obterTodosClientes?Nome=ana&Status=1&Pagina=1&TamanhoPagina=10`. Busca por parte do nome e status (0 inativo, 1 ativo), ordenada por nome e ID.
- Reservas: `GET /api/v1/reservas?Status=0&Pagina=1&TamanhoPagina=10`. Também aceita `ClienteId` e `ImovelId`. Status: 0 pendente, 1 confirmada, 2 concluída, 3 cancelada. Ordenação por data de entrada e ID.

Parâmetros de paginação ou status inválidos retornam HTTP 400. Os filtros e a paginação são aplicados no banco antes de carregar os registros.

## Regras de negócio principais

- CPF deve ser válido e conter apenas números; CPF e e-mail não podem ser duplicados.
- Clientes inativos não podem realizar reservas.
- Uma reserva precisa respeitar o período permitido, a capacidade do imóvel e a disponibilidade do imóvel.
- A reserva possui fluxo de status: pendente, confirmada, concluída ou cancelada.
- O valor total considera a duração da estadia e a política de desconto configurada.
- O cancelamento calcula o reembolso conforme a política de antecedência.
- Alterações que deixariam um imóvel incompatível com reservas ativas são rejeitadas.

## Configuração do desconto

Em `src/LocacaoImoveis.API/appsettings.json`:

```json
"Desconto": {
  "MinimoNoites": 7,
  "Percentual": 10
}
```

Nesse exemplo, reservas com pelo menos sete noites recebem dez por cento de desconto.

## Documentação complementar

- [Casos de uso e exemplos da API](docs/casos-de-uso.md)
- [Arquitetura e classes](docs/classes-e-arquitetura.md)
- [Fluxos de execução](docs/sequencias.md)
- [Rastreabilidade, testes e pendências](docs/rastreabilidade.md)
- [Checklist de implementação](docs/CHECKLIST.md)
- [Diagramas UML](docs/uml/galeria.html)
- [Registro da documentação](docs/registro-da-documentacao.md)
- [Uso de IA no projeto](AI_USAGE.md)

## Limitações atuais

- A persistência é temporária porque o banco é SQLite em memória.
- Não há autenticação, autorização ou configuração de produção. As listagens paginadas retornam somente os itens, sem total de registros ou páginas.
- O seed é destinado ao desenvolvimento e aos testes locais.
- O comportamento implementado deve ser conferido junto ao código e aos testes; os diagramas e o PDF descrevem o desafio e podem conter requisitos ainda não implementados.
