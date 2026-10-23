using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.Services;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace LocacaoImoveis.API.Controllers
{
    /// <summary>Operações HTTP de clientes.</summary>
    [ApiController]
    [Route("api/v1/clientes")]
    public class ClienteController : ControllerBase
    {

        private readonly ClienteService _clienteService; // dependência do serviço de cliente
        /// <summary>Inicializa o controller com o serviço de aplicação.</summary>
        /// <param name="clienteService">Serviço responsável pelas operações do recurso.</param>
        public ClienteController(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        /// <summary>Cadastra um cliente com CPF e e-mail únicos.</summary>
        /// <param name="request">Nome, telefone, e-mail e CPF do cliente.</param>
        /// <response code="201">Cliente criado; retorna o ID e o endereço de consulta.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="409">CPF ou e-mail já cadastrado.</response>
        [HttpPost]
        [Route("criarCliente")]
        [EndpointSummary("Cadastra um novo cliente")]
        [EndpointDescription("Cria um cliente validando CPF, email e os dados obrigatorios.")]
        [ProducesResponseType(StatusCodes.Status201Created)] // Indica que a resposta será 201 Created
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Indica que a resposta será 400 Bad Request
        [ProducesResponseType(StatusCodes.Status409Conflict)] // caso de 2 cpfs iguais
        public async Task<IActionResult> CriarCliente(
            [FromBody] CriarClienteRequestDTO request)
        {

            var clienteId = await _clienteService.CriarClienteAsync(request);

            // Retorna 201 Created com o ID do cliente criado
            return CreatedAtAction(
                nameof(ObterClientePorId),
                new { id = clienteId },
                new { id = clienteId });

        }

        /// <summary>Consulta um cliente pelo ID.</summary>
        /// <param name="id">Identificador do cliente.</param>
        /// <response code="200">Dados do cliente.</response>
        /// <response code="404">Cliente não encontrado.</response>
        [HttpGet("{id:guid}/detalhes")]
        [EndpointSummary("Busca um cliente pelo ID")]
        [EndpointDescription("Retorna os dados do cliente identificado pelo GUID informado.")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)] // Indica que a resposta será 200 OK
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Indica que a resposta será 404 Not Found
        public async Task<IActionResult> ObterClientePorId(Guid id)
        {
            var cliente = await _clienteService.ObterClientePorIdAsync(id);
            return Ok(cliente);
        }

        /// <summary>Consulta um cliente pelo CPF.</summary>
        /// <param name="cpf">CPF do cliente.</param>
        /// <response code="200">Dados do cliente.</response>
        /// <response code="400">CPF inválido.</response>
        /// <response code="404">Cliente não encontrado.</response>
        [HttpGet("{cpf}/detalhes")]
        [EndpointSummary("Busca um cliente pelo CPF")]
        [EndpointDescription("Retorna os dados do cliente identificado pelo CPF informado.")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorCpfAsync(string cpf)
        {

            var cliente = await _clienteService.ObterClientePorCpfAsync(cpf);
            return Ok(cliente);

        }

        /// <summary>Lista clientes com filtros e paginação.</summary>
        /// <param name="filtro">Parte do nome, status, página e tamanho da página.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="200">Clientes da página solicitada; lista vazia quando não há resultados.</response>
        /// <response code="400">Filtros ou paginação inválidos.</response>
        [HttpGet]
        [Route("obterTodosClientes")]
        [EndpointSummary("Lista clientes com filtros e paginação")]
        [EndpointDescription("Filtra por parte do nome e status. Página padrão 1, tamanho padrão 10 e máximo 100. Ordena por nome e ID.")]
        [ProducesResponseType(typeof(List<ClienteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObterTodosClientes([FromQuery] ListarClientesRequest filtro, CancellationToken cancellationToken)
        {

            var clientes = await _clienteService.ObterTodosClientesAsync(filtro, cancellationToken);


            return Ok(clientes);

        }

        /// <summary>Atualiza o nome e o telefone de um cliente.</summary>
        /// <param name="id">Identificador do cliente.</param>
        /// <param name="request">Novo nome e telefone.</param>
        /// <response code="204">Cliente atualizado.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="404">Cliente não encontrado.</response>
        [HttpPut("{id:guid}/atualizar")]
        [EndpointSummary("Atualiza um cliente")]
        [EndpointDescription("Atualiza os dados do cliente identificado pelo GUID informado.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtualizarClienteRequest(Guid id, [FromBody] AtualizarClienteRequest request)
        {

            // Chama o serviço para atualizar o cliente
            await _clienteService.AtualizarClienteAsync(id, request);

            return NoContent();

        }

        // altera somente o status do cliente
        /// <summary>Ativa um cliente.</summary>
        /// <param name="id">Identificador do cliente.</param>
        /// <response code="204">Cliente ativado.</response>
        /// <response code="404">Cliente não encontrado.</response>
        [HttpPut("{id:guid}/ativar")]
        [EndpointSummary("Ativa um cliente")]
        [EndpointDescription("Altera o status do cliente para ativo.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtivarCliente(Guid id)
        {
            await _clienteService.AtivarClienteAsync(id);

            return NoContent();
        }

        // altera somente o status do cliente para inativo
        /// <summary>Inativa um cliente sem reservas ativas.</summary>
        /// <param name="id">Identificador do cliente.</param>
        /// <response code="204">Cliente inativado.</response>
        /// <response code="404">Cliente não encontrado.</response>
        /// <response code="409">Cliente possui reservas ativas.</response>
        [HttpPut("{id:guid}/inativar")]
        [EndpointSummary("Inativa um cliente")]
        [EndpointDescription("Altera o status do cliente para inativo.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InativarCliente(Guid id)
        {
            await _clienteService.InativarClienteAsync(id);
            return NoContent();
        }


    }
}
