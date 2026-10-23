# Arquitetura e classes

## Visao geral

O projeto usa quatro projetos separados. A API recebe HTTP, a Application coordena os casos de uso, o Domain concentra regras e entidades e a Infrastructure acessa o SQLite por meio do Entity Framework Core.

```text
API -> Application -> Domain
 |          |
 +------> Infrastructure -> Application/Domain
```

| Projeto | Responsabilidade |
|---|---|
| `LocacaoImoveis.Domain` | Entidades, enums, value objects, validacao e politicas |
| `LocacaoImoveis.Application` | DTOs, servicos, interfaces e excecoes |
| `LocacaoImoveis.Infrastructure` | DbContext, repositorios e seed |
| `LocacaoImoveis.API` | Controllers, middleware, DI, OpenAPI e health |

O Domain nao referencia as outras camadas. A Application conhece o Domain, mas nao conhece EF Core. A Infrastructure implementa as interfaces definidas pela Application. A API monta a injecao de dependencia em `Program.cs`.

## Domain

### Cliente

`Cliente` possui `Id`, nome, telefone, `Cpf`, `Email` e `Status`. O construtor publico valida os dados e cria o cliente como `Ativo`. O construtor privado existe para o EF Core reconstruir a entidade a partir do banco.

### Imovel

`Imovel` possui endereco, titulo, capacidade, valor da diaria e `ImovelStatus`. O construtor e o metodo de atualizacao validam textos nao vazios e valores positivos. O status inicial e `Disponivel`.

### Reserva

`Reserva` guarda cliente, imovel, periodo, hospedes, valor total, status, data de criacao e os dados de cancelamento. O construtor valida datas, limite de 30 dias, hospedes e valor maior que zero. Os metodos `Confirmar`, `Concluir` e `Cancelar` controlam as transicoes permitidas.

### Value Objects

`Cpf` e `Email` sao `sealed record` imutaveis. O CPF normaliza a entrada removendo pontos e hifens, exige 11 numeros e rejeita uma sequencia com todos os digitos iguais. Depois, `ValidaCPF.IsCpf` calcula os dois digitos verificadores: usa os pesos de 10 a 2 nos nove primeiros digitos, calcula o primeiro digito pelo resto da divisao por 11, inclui esse resultado e repete o calculo com os pesos de 11 a 2. O valor so e aceito quando os dois digitos calculados sao iguais aos dois ultimos digitos informados.

O e-mail normaliza espacos e letras maiusculas e usa `MailAddress.TryCreate`. O EF Core converte CPF e e-mail para `string` no banco e reconstrui os objetos na leitura.

### Politicas

`DescontoPorDuracao` implementa `IPoliticaDesconto` e calcula o total conforme a quantidade de noites. Os valores vem de `Desconto:MinimoNoites` e `Desconto:Percentual`, com padrao 7 e 10.

`PoliticaDeReembolso` calcula o valor pelo tempo entre o cancelamento e a entrada: acima de 7 dias, 100%; de 2 a 7 dias, 50%; abaixo de 2 dias, zero.

## Application

Os DTOs evitam expor diretamente as entidades. Existem requests para criar e atualizar clientes e imoveis, filtrar imoveis, consultar disponibilidade e criar reservas. As respostas `ClienteResponse`, `ImovelResponse` e `ReservaResponse` definem os dados que saem da API.

`ClienteService`, `ImovelService` e `ReservaService` coordenam validacoes, consultas, regras e mapeamento para DTOs. Eles recebem interfaces de repositorio por injecao de dependencia.

`IClienteRepository`, `IImovelRepository` e `IReservaRepository` representam as operacoes de persistencia. As excecoes de aplicacao representam conflito e recurso nao encontrado.

## Infrastructure

`AppDbContext` possui os conjuntos `Clientes`, `Imoveis` e `Reservas`. Ele configura chaves, campos obrigatorios, conversao de CPF e e-mail, indices unicos e chaves estrangeiras de reserva.

Os repositorios usam consultas assincronas do EF Core. Conflitos de reserva consideram somente estados `Pendente` e `Confirmada`, usando a regra de sobreposicao:

```text
entradaExistente < saidaSolicitada
e saidaExistente > entradaSolicitada
```

`DbInitializer` cria tres clientes, tres imoveis e duas reservas quando o banco ainda esta vazio. Uma reserva do seed permanece pendente, como qualquer reserva criada pelo servico.

## API

Os controllers possuem a rota base `/api/v1`. Os metadados `EndpointSummary`, `EndpointDescription` e `ProducesResponseType` alimentam o OpenAPI. O `OpenApiExamplesTransformer` adiciona exemplos de request e response para clientes, imoveis, reservas e health.

O middleware de excecoes converte falhas da aplicacao em respostas HTTP. O `HealthController` executa os health checks registrados e nao verifica o banco.

## Assincronia e cancelamento

Operacoes de banco usam `Task` e metodos `Async` do EF Core. Controllers de imovel e reserva recebem `CancellationToken` e o repassam para servicos e repositorios nas operacoes que aceitam o token. A criacao de reserva tambem usa o token ao aguardar o `SemaphoreSlim`.

## Injecao de dependencia

Repositorios e servicos sao registrados como `Scoped`, acompanhando o ciclo de uma requisicao. A conexao SQLite em memoria e `Singleton` para permanecer aberta durante a vida da API. `AppDbContext` e criado por escopo. A politica de desconto e `Singleton` porque seus parametros sao imutaveis apos a configuracao.
