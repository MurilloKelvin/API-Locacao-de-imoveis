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

`Cliente` possui `Id`, nome, telefone, `DocumentoCliente`, `Email`, `CriadoEm` e `Status`. O construtor publico valida os dados e cria o cliente como `Ativo`. O construtor privado existe para o EF Core reconstruir a entidade a partir do banco.

### Imovel

`Imovel` possui endereco, titulo, capacidade, valor da diaria e `ImovelStatus`. O construtor e o metodo de atualizacao validam textos nao vazios e valores positivos. O status inicial e `Disponivel`.

### Reserva

`Reserva` guarda cliente, imovel, periodo, hospedes, valor total, status, data de criacao e os dados de cancelamento. O construtor valida datas, limite de 30 dias, hospedes e valor maior que zero. Os metodos `Confirmar`, `Concluir` e `Cancelar` controlam as transicoes permitidas.

### Value Objects

`Cpf` e `Email` sao `sealed record` imutaveis. O CPF normaliza a entrada removendo pontos e hifens, exige 11 numeros e rejeita uma sequencia com todos os digitos iguais. Depois, `ValidaCPF.IsCpf` calcula os dois digitos verificadores: usa os pesos de 10 a 2 nos nove primeiros digitos, calcula o primeiro digito pelo resto da divisao por 11, inclui esse resultado e repete o calculo com os pesos de 11 a 2. O valor so e aceito quando os dois digitos calculados sao iguais aos dois ultimos digitos informados.

O e-mail normaliza espacos e letras maiusculas e usa `MailAddress.TryCreate`. O EF Core converte documento e e-mail para `string` no banco e reconstrui os objetos na leitura.

### Politicas

`DescontoPorDuracao` implementa `IPoliticaDesconto` e calcula o total conforme a quantidade de noites. Os valores vem de `Desconto:MinimoNoites` e `Desconto:Percentual`, com padrao 7 e 10.

`PoliticaDeReembolso` calcula o valor pelo tempo entre o cancelamento e a entrada: acima de 7 dias, 100%; de 2 a 7 dias, 50%; abaixo de 2 dias, zero.

## Application

Os DTOs evitam expor diretamente as entidades. Existem requests para criar e atualizar clientes e imoveis, filtrar as listagens de clientes, imoveis e reservas, consultar disponibilidade e criar reservas. As respostas `ClienteResponse`, `ImovelResponse` e `ReservaResponse` definem os dados que saem da API.

`ListarClientesRequest` aceita parte do nome, status, página e tamanho da página. `ListarReservasRequest` aceita IDs de cliente e imóvel, status, página e tamanho da página. As duas listagens usam página 1 e tamanho 10 por padrão, com máximo de 100 itens. Os serviços validam os valores; os repositórios aplicam filtros, ordenação e `Skip`/`Take` no SQLite antes de materializar os resultados. Clientes são ordenados por nome e ID; reservas, por data de entrada e ID. As respostas são listas, sem contagem total.

`ClienteService`, `ImovelService` e `ReservaService` coordenam validacoes, consultas, regras e mapeamento para DTOs. Eles recebem interfaces de repositorio por injecao de dependencia.

`IClienteRepository`, `IImovelRepository` e `IReservaRepository` representam as operacoes de persistencia. As excecoes de aplicacao representam conflito e recurso nao encontrado.

## Infrastructure

`AppDbContext` possui os conjuntos `Clientes`, `Imoveis` e `Reservas`. Ele configura chaves, campos obrigatorios, conversao de documento e e-mail, indices unicos e chaves estrangeiras de reserva.

Os repositorios usam consultas assincronas do EF Core. Conflitos de reserva consideram somente estados `Pendente` e `Confirmada`, usando a regra de sobreposicao:

```text
entradaExistente < saidaSolicitada
e saidaExistente > entradaSolicitada
```

`DbInitializer` cria cinco clientes, quatro imoveis e tres reservas quando o banco ainda esta vazio. Uma reserva do seed permanece pendente, como qualquer reserva criada pelo servico.

## API

Os controllers possuem a rota base `/api/v1`. Os metadados `EndpointSummary`, `EndpointDescription` e `ProducesResponseType` alimentam o OpenAPI. O `OpenApiExamplesTransformer` adiciona exemplos de request e response para clientes, imoveis, reservas e health.

O middleware de excecoes converte falhas da aplicacao em respostas HTTP. O `HealthController` executa os health checks registrados e nao verifica o banco.

## Assincronia e cancelamento

Operacoes de banco usam `Task` e metodos `Async` do EF Core. Controllers de imovel e reserva recebem `CancellationToken` e o repassam para servicos e repositorios nas operacoes que aceitam o token. A criacao de reserva tambem usa o token ao aguardar o `SemaphoreSlim`.

## Injecao de dependencia

Repositorios e servicos sao registrados como `Scoped`, acompanhando o ciclo de uma requisicao. A conexao SQLite em memoria e `Singleton` para permanecer aberta durante a vida da API. `AppDbContext` e criado por escopo. A politica de desconto e `Singleton` porque seus parametros sao imutaveis apos a configuracao.

## Documento, telefone e atividade

`DocumentoCliente` é um objeto de valor que escolhe `Cpf` para 11 caracteres alfanuméricos ou `Cnpj` para 14. CPF calcula os dígitos verificadores. CNPJ remove pontuação, converte letras para maiúsculas e exige 12 caracteres alfanuméricos seguidos de dois números; atualmente não calcula dígitos verificadores. A propriedade persistida é `DocumentoCliente`, com índice único e tamanho máximo de 14 caracteres.

`ValidaTelefone.ValidarTelefone` exige a expressão `^\d{10,11}$` antes de aparar espaços. Portanto, envie somente dígitos, sem espaços, DDI, parênteses ou hífen. A entidade guarda o telefone como `string` e valida na criação e atualização.

## Entradas REST e GraphQL

Controllers e resolvers `Query`/`Mutation` chamam os mesmos services. GraphQL usa Hot Chocolate em `/graphql`. Consultas leem dados; mutations criam, alteram estados ou emitem token. A autorização é aplicada nos controllers de clientes/reservas e nos respectivos campos GraphQL; operações de imóveis são públicas.

`JwtOptions` recebe a seção `Jwt`; `TokenService` é Singleton e emite JWT HS256 com `sub` igual ao ClientId e claim de papel `api-client`. `AuthController.Token` e `Mutation.Login` conferem as credenciais e chamam o mesmo gerador. Não há usuários, senhas individuais, refresh token ou restrição por papel implementados. A validação confere assinatura, emissor, audience e validade, com tolerância de 30 segundos.

## Inativação automática

`InativarClientesJob` deriva de `BackgroundService`, executa após a inicialização e repete a cada 24 horas enquanto a API está ligada. Cria um escopo para acessar `AppDbContext` diretamente; não passa por `ClienteService`. Carrega clientes ativos e reservas, preserva quem possui reserva pendente/confirmada e calcula a última atividade a partir de `CriadoEm`, saída de reserva concluída ou cancelamento (com fallback para criação). Inativa quando essa data é menor ou igual ao dia UTC de três meses atrás. O seed inclui um cliente de quatro meses sem reservas para demonstrar a regra.

## Limites do tratamento de erros

`ExceptionHandlingMiddleware` usa `IProblemDetailsService` no REST: argumentos inválidos viram 400, recurso ausente 404, duplicidades e operações de negócio inválidas 409, demais exceções 500 com mensagem genérica. O repositório atual não traduz `DbUpdateException` de unicidade: uma disputa de gravação pode chegar como 500. GraphQL usa seu próprio envelope `errors`, e não deve ser descrito como uma resposta REST ProblemDetails.
