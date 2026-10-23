# Sequencias de execucao

Os passos abaixo mostram o caminho de uma requisicao pela API. Os controllers recebem HTTP, os servicos aplicam regras e os repositorios acessam o `AppDbContext`.

## Criar cliente

```text
Cliente -> API: POST /api/v1/clientes/criarCliente
API -> ClienteService: CriarClienteAsync(request)
ClienteService -> Cpf/Email: normalizar e validar
ClienteService -> ClienteRepository: verificar CPF e e-mail
ClienteService -> Cliente: criar entidade
ClienteService -> ClienteRepository: adicionar e salvar
ClienteService -> API: retornar Guid
API -> Cliente: 201 Created com id
```

Se a validacao falhar, o servico interrompe o fluxo e o middleware monta a resposta de erro.

## Consultar cliente ou imovel

```text
Cliente -> API: GET /api/v1/recurso/{id}/detalhes
API -> Service: receber Guid
Service -> Repository: consultar por FindAsync
Repository -> Service: entidade ou null
Service -> DTO: mapear resposta
Service -> API: response
API -> Cliente: 200 ou 404
```

Consulta por CPF normaliza o CPF antes da busca. O framework rejeita valores que nao podem ser convertidos para Guid.

## Listar clientes

O controller recebe `ListarClientesRequest`, o serviço valida página, tamanho (1 a 100) e status, e o repositório filtra por parte do nome e status. O SQLite ordena por nome e ID e aplica `Skip`/`Take`. O serviço transforma as entidades em `ClienteResponse`. A API retorna `200` com a lista (ou `[]`) e `400` para filtros inválidos. Veja o diagrama `20-sequencia-listar-clientes`.

## Listar reservas

O controller recebe `ListarReservasRequest`, o serviço valida página, tamanho (1 a 100) e status, e o repositório filtra por cliente, imóvel e status. O SQLite ordena por data de entrada e ID e aplica `Skip`/`Take`. O serviço transforma as entidades em `ReservaResponse`. A API retorna `200` com a lista (ou `[]`) e `400` para filtros inválidos. Veja o diagrama `21-sequencia-listar-reservas`.

As duas listagens usam página 1 e tamanho 10 por padrão e não retornam a contagem total.

## Listar imoveis

```text
Cliente -> API: GET /api/v1/imoveis/listar?filtros
API -> ImovelService: ListarImoveisAsync
ImovelService: validar pagina e tamanho
ImovelService -> ImovelRepository: aplicar filtros, ordenacao, Skip e Take
ImovelRepository -> ImovelService: lista de entidades
ImovelService -> API: lista de ImovelResponse
API -> Cliente: 200 com lista
```

Sem parametros, a pagina e 1 e o tamanho e 10. A consulta de disponibilidade segue o mesmo caminho, mas aplica o periodo e ignora reservas pendentes ou confirmadas que conflitam.

## Criar reserva

```text
Cliente -> API: POST /api/v1/reservas
API -> ReservaService: CriarReservaAsync(request, token)
ReservaService: aguardar SemaphoreSlim
ReservaService -> ClienteRepository: buscar cliente
ReservaService: conferir ativo e limite de 3 reservas
ReservaService -> ImovelRepository: buscar imovel
ReservaService: conferir status e capacidade
ReservaService -> ReservaRepository: verificar conflito do periodo
ReservaService -> PoliticaDesconto: calcular total
ReservaService -> Reserva: criar como Pendente
ReservaService -> ReservaRepository: adicionar e salvar
ReservaService: liberar SemaphoreSlim
ReservaService -> API: retornar Guid
API -> Cliente: 201 Created com id
```

O lock serializa a criacao dentro da instancia da API. O conflito usa intervalos abertos nas extremidades: uma saida no mesmo dia da entrada seguinte nao e considerada sobreposicao.

## Confirmar ou concluir reserva

```text
Cliente -> API: PUT /api/v1/reservas/{id}/confirmar
API -> ReservaService: ConfirmarReservaAsync
ReservaService -> ReservaRepository: buscar reserva
ReservaService -> Reserva: Confirmar
ReservaService -> ReservaRepository: SaveChangesAsync
API -> Cliente: 204 No Content
```

Concluir usa o mesmo caminho e chama `Reserva.Concluir`. A entidade rejeita confirmacao de uma reserva que nao esteja pendente e conclusao de uma que nao esteja confirmada.

## Cancelar reserva

```text
Cliente -> API: PUT /api/v1/reservas/{id}/cancelar
API -> ReservaService: CancelarReservaAsync
ReservaService -> ReservaRepository: buscar reserva
ReservaService: obter agora no fuso UTC-3
ReservaService: considerar entrada as 14:00
ReservaService -> PoliticaDeReembolso: calcular valor
ReservaService -> Reserva: Cancelar(agora, reembolso)
ReservaService -> ReservaRepository: SaveChangesAsync
API -> Cliente: 204 No Content
```

O valor de reembolso fica salvo em `ValorReembolso`. Reservas concluidas ou ja canceladas nao podem ser canceladas novamente.

## Health

```text
Cliente -> API: GET /health
API -> HealthCheckService: CheckHealthAsync
HealthCheckService -> API: Healthy ou outro estado
API -> Cliente: 200 ou 503 com status
```

O health atual nao consulta banco. Ele executa somente os checks registrados em `AddHealthChecks()`.
