# Uso de inteligência artificial

Este arquivo também foi elaborado com auxílio de inteligência artificial, a partir do contexto real do código-fonte e dos testes do projeto.

Foram utilizados o GitHub Copilot (student) e o ChatGPT na versão web como ferramentas de apoio ao desenvolvimento. A IA foi usada como apoio para compreender problemas, obter explicações e avaliar possíveis formas de implementação, mas não substituiu a autoria e a validação manual do código.

## Contexto da aplicação

O projeto é uma API ASP.NET Core para locação de imóveis, organizada em camadas. O domínio contém as entidades e regras de negócio de clientes, imóveis e reservas. A camada de aplicação concentra os serviços, DTOs, exceções, interfaces de repositório e políticas de desconto e reembolso. A infraestrutura utiliza Entity Framework Core com SQLite em memória, repositórios e seed de dados. A API expõe os endpoints por meio de controllers e possui um middleware para converter exceções da aplicação em respostas HTTP padronizadas usando `ProblemDetails`.

Entre as regras implementadas estão a criação, confirmação, conclusão e cancelamento de reservas, o limite de reservas ativas por cliente, a verificação de disponibilidade e conflitos de período, o cálculo do valor da reserva, o desconto por duração e o cálculo do reembolso de acordo com a antecedência do cancelamento. Os testes estão separados entre testes de domínio e testes de integração, incluindo chamadas HTTP reais contra a aplicação por meio de `WebApplicationFactory<Program>`.

## Apoio utilizado

| Situação | Apoio prestado pela IA |
|---|---|
| Contextualização do projeto | Foi solicitado que a IA explicasse o contexto geral da aplicação, suas camadas e a responsabilidade das principais partes do código. |
| Estrutura inicial da aplicação | Foi solicitado um esquema inicial de arquitetura e de organização de pastas para separar API, aplicação, domínio, infraestrutura e testes. |
| Bugs na implementação do reembolso | Foram feitas perguntas sobre as regras da `PoliticaDeReembolso`, os intervalos de antecedência e os problemas encontrados durante a implementação e os testes. |
| Bugs relacionados ao desconto | Foram solicitadas explicações sobre a política de desconto por duração, o cálculo percentual e as condições para aplicar ou não o desconto. |
| Uso de factory para testar a API | A IA explicou como utilizar `WebApplicationFactory<Program>`, como expor o `Program` para os testes e como criar um `HttpClient` para testar a aplicação hospedada em memória. |
| Testes HTTP e testes de reserva | A IA ajudou a entender como estruturar testes de integração para consultar clientes e imóveis, criar reservas por HTTP e verificar o comportamento de duas tentativas concorrentes para o mesmo período. |
| Configuração do middleware | Foram solicitadas orientações para registrar o middleware de tratamento de exceções e transformar exceções de negócio em respostas HTTP com status e `ProblemDetails`. |
| Configuração inicial do banco de dados | A configuração inicial do SQLite em memória, do `AppDbContext`, da criação das tabelas e do seed foi apoiada principalmente pelo autocomplete do Visual Studio. A IA não foi usada para substituir a implementação manual dessa configuração. |
| Bug de SQL com `OrderBy` direto em `criadaEm` | No commit `20c6e95`, foi solicitada uma explicação sobre o problema causado pelo `OrderBy` aplicado diretamente a `criadaEm` e sobre o que deveria ser feito para corrigi-lo. |

## Participação na implementação

A implementação inicial do código funcional da aplicação — incluindo as entidades, regras de negócio, serviços, controllers, repositórios, políticas, middleware e configurações — foi escrita manualmente. Nessa etapa, a IA foi utilizada principalmente para explicar conceitos, investigar bugs e sugerir caminhos de implementação. Em alguns casos, quando uma explicação não era suficiente, a IA apresentava parte do código como exemplo; a implementação final era então concluída, adaptada e validada manualmente.

Posteriormente, os testes de filtro e paginação foram feitos com IA e auditados manualmente.

Alguns testes, especialmente os testes HTTP e de reserva mencionados acima, foram gerados ou tiveram sua estrutura inicial criada com auxílio de IA. A aplicação, suas regras de negócio e a maior parte dos testes de domínio e integração foram implementadas manualmente. As sugestões recebidas foram revisadas e ajustadas para refletir o comportamento efetivo da aplicação.

## Apoio na documentacao

| Situacao | Apoio prestado pela IA |
|---|---|
| Criacao do OpenAPI | A IA ajudou a criar a documentacao OpenAPI, os exemplos de request e response exibidos no Swagger e o transformer usado pela API. |
| Explicacao do OpenAPI | A IA explicou o que e o OpenAPI, como ele e gerado e como os exemplos aparecem na documentacao do Swagger. |
| Criacao do README | A IA ajudou a organizar e escrever o README com instrucoes para executar o projeto, acessar o Swagger, consultar os endpoints e executar os testes. |
| Criação da galeria de diagramas UML | A galeria personalizada em `docs/uml/galeria.html`, incluindo os recursos de navegação e zoom, foi criada com auxílio de IA. O HTML não é gerado pelo PlantUML: ele exibe as imagens SVG e PNG que o PlantUML gera a partir dos arquivos `.puml`. |
