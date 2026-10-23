# Guia da API e casos de uso

## Conceitos basicos

O consumidor usa Swagger, curl ou outro cliente HTTP. O prefixo dos recursos e `/api/v1`. Os exemplos mostram o formato dos dados; os IDs precisam ser substituidos por valores existentes.

| Recurso | Metodo e rota | Resultado principal |
|---|---|---|
| Cliente | `POST /api/v1/clientes/criarCliente` | Cria e retorna `201` com ID |
| Cliente | `GET /api/v1/clientes/{id:guid}/detalhes` | Consulta por GUID |
| Cliente | `GET /api/v1/clientes/{cpf}/detalhes` | Consulta por CPF |
| Cliente | `GET /api/v1/clientes/obterTodosClientes` | Lista com filtros e paginação |
| Cliente | `PUT /api/v1/clientes/{id:guid}/atualizar` | Atualiza e retorna `204` |
| Cliente | `PUT /api/v1/clientes/{id:guid}/ativar` | Ativa e retorna `204` |
| Cliente | `PUT /api/v1/clientes/{id:guid}/inativar` | Inativa e retorna `204` |
| Imovel | `POST /api/v1/imoveis` | Cria e retorna `201` com ID |
| Imovel | `GET /api/v1/imoveis/{id:guid}/detalhes` | Consulta por GUID |
| Imovel | `GET /api/v1/imoveis/listar` | Lista com filtros e paginação |
| Imovel | `GET /api/v1/imoveis/imoveisDisponiveis` | Consulta disponibilidade |
| Imovel | `PUT /api/v1/imoveis/{id:guid}/atualizarImovel` | Atualiza e retorna `204` |
| Imovel | `PUT /api/v1/imoveis/{id:guid}/atualizarStatus` | Altera status |
| Reserva | `POST /api/v1/reservas` | Cria e retorna `201` com ID |
| Reserva | `GET /api/v1/reservas` | Lista com filtros e paginação |
| Reserva | `GET /api/v1/reservas/{id:guid}/detalhes` | Consulta por GUID |
| Reserva | `PUT /api/v1/reservas/{id:guid}/confirmar` | Pendente para Confirmada |
| Reserva | `PUT /api/v1/reservas/{id:guid}/concluir` | Confirmada para Concluida |
| Reserva | `PUT /api/v1/reservas/{id:guid}/cancelar` | Calcula reembolso e cancela |
| API | `GET /health` | Retorna o estado da API |

## Cliente

Para criar, envie:

```json
{
  "nome": "Ana Exemplo",
  "telefone": "31999999999",
  "email": "ana@example.com",
  "cpf": "529.982.247-25"
}
```

O nome e aparado. O CPF remove pontos e hifens, exige 11 numeros, rejeita todos os digitos iguais e calcula os dois digitos verificadores pelo modulo 11. O primeiro calculo usa pesos de 10 a 2; o segundo usa pesos de 11 a 2. O CPF so e aceito quando os resultados conferem com os dois ultimos digitos informados. Depois, ele e armazenado como objeto de valor. O e-mail e aparado, convertido para minusculas e validado. O cliente nasce como `Ativo`. CPF e e-mail possuem indices unicos no banco.

Nome, CPF e e-mail invalidos produzem erro `400`. CPF ou e-mail duplicado produz conflito. Consultas inexistentes produzem `404`. A listagem aceita `Nome`, `Status`, `Pagina` e `TamanhoPagina` como parâmetros de consulta e retorna `200` com uma lista; sem resultados, retorna `[]`.

Exemplo: `GET /api/v1/clientes/obterTodosClientes?Nome=ana&Status=1&Pagina=1&TamanhoPagina=10`. O nome é buscado por parte do texto, sem diferenciar maiúsculas de minúsculas. A ordenação é por nome e ID.

Atualizacao recebe somente nome e telefone. CPF, e-mail, ID e status nao sao alterados por esse endpoint. Ativar e inativar nao precisam de corpo. As operacoes de escrita sao assincronas e as excecoes passam pelo middleware global.

## Imovel

Para criar, envie:

```json
{
  "endereco": "Rua Exemplo, 10",
  "titulo": "Casa para temporada",
  "capacidadeDeHospedes": 4,
  "valorDiaria": 250.50
}
```

Endereco e titulo nao podem ser vazios. Capacidade e valor da diaria devem ser maiores que zero. O imovel nasce como `Disponivel`.

A listagem usa `pagina` e `tamanhoPagina`, com padrao 1 e 10. O tamanho aceito vai de 1 a 100. Tambem aceita `status`, `capacidadeMinima`, `capacidadeMaxima`, `valorDiarioMinimo` e `valorDiarioMaximo`.

Exemplo:

```text
GET /api/v1/imoveis/listar?pagina=1&tamanhoPagina=10&status=1&capacidadeMinima=4&valorDiarioMaximo=300
```

A consulta de disponibilidade recebe data de entrada, data de saida e quantidade de hospedes. Quando as datas nao sao informadas, a entrada usa o dia atual no fuso UTC-3 e a saida usa 30 dias depois. Ela considera imoveis disponiveis, capacidade e conflitos com reservas pendentes ou confirmadas. A consulta nao reserva o imovel; a criacao ainda precisa repetir a verificacao.

Status do imovel: `0` Indisponivel, `1` Disponivel e `2` EmManutencao. Para mudar o status, o novo valor deve ser valido e diferente do atual. A alteracao nao pode deixar um imovel com reserva ativa em situacao incompativel, conforme a regra do servico.

## Reserva

A listagem aceita `ClienteId`, `ImovelId`, `Status`, `Pagina` e `TamanhoPagina` como parâmetros opcionais. Exemplo: `GET /api/v1/reservas?Status=0&Pagina=1&TamanhoPagina=10`. A ordenação é por data de entrada e ID. O filtro por período ainda não está implementado.

Nas listagens de clientes e reservas, a página padrão é 1, o tamanho padrão é 10 e o tamanho máximo é 100. Parâmetros inválidos retornam `400`. A resposta não inclui total de registros nem total de páginas.

Para criar, envie:

```json
{
  "clienteId": "00000000-0000-0000-0000-000000000001",
  "imovelId": "00000000-0000-0000-0000-000000000002",
  "dataEntrada": "2030-01-10",
  "dataSaida": "2030-01-13",
  "quantidadeHospedes": 2
}
```

O servico confirma que cliente e imovel existem, que o cliente esta ativo, que o imovel esta disponivel, que a capacidade suporta os hospedes, que o cliente nao possui 3 reservas ativas e que o periodo nao conflita com outra reserva ativa. A entrada deve ser antes da saida, nao pode estar no passado e a estadia nao pode passar de 30 dias.

O total e calculado pelas noites multiplicadas pela diaria. A politica configurada aplica desconto para a quantidade minima de noites definida em `appsettings.json`; o padrao e 7 noites e 10 por cento.

Toda reserva nova nasce como `Pendente`. O fluxo de status e:

```text
Pendente -> Confirmada -> Concluida
Pendente ou Confirmada -> Cancelada
```

Somente pendentes podem ser confirmadas. Somente confirmadas podem ser concluidas. Somente pendentes ou confirmadas podem ser canceladas. O cancelamento calcula reembolso pela politica de antecedencia e grava `CanceladaEm` e `ValorReembolso`.

As faixas atuais de reembolso sao:

| Tempo ate a entrada | Reembolso |
|---|---:|
| Mais de 7 dias | 100% |
| De 2 a 7 dias, inclusive | 50% |
| Menos de 2 dias | 0% |

O servico usa um `SemaphoreSlim` estatico para serializar a criacao de reservas no processo. Isso evita que duas requisicoes concorrentes criem reservas conflitantes dentro da mesma instancia da API.

## Health

`GET /health` executa os health checks registrados e retorna apenas o status geral. Atualmente nao ha check de banco ou de servico externo registrado. O resultado normal e:

```json
{
  "status": "Healthy"
}
```

## Erros

O middleware global converte excecoes em respostas HTTP. A classificacao exata depende da excecao lancada pelo servico, por isso o Swagger deve ser consultado junto com o codigo para ver os status declarados em cada endpoint. IDs precisam ser GUIDs validos; valores que nao podem ser convertidos sao rejeitados pelo framework antes do controller.
