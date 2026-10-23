using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.Services;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace LocacaoImoveis.API.Controllers
{
    [ApiController]
    [Route("api/v1/clientes")]
    public class ClienteController : ControllerBase
    {

        private readonly ClienteService _clienteService; // dependência do serviço de cliente
        public ClienteController(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

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
