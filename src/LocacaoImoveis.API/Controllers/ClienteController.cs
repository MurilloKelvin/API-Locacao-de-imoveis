using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.Services;
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
        [ProducesResponseType(StatusCodes.Status200OK)] // Indica que a resposta será 200 OK
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Indica que a resposta será 404 Not Found
        public async Task<IActionResult> ObterClientePorId(Guid id)
        {
            var cliente = await _clienteService.ObterClientePorIdAsync(id);
            if (cliente == null)
                return NotFound(new { message = "Cliente não encontrado." });

            return Ok(cliente);
        }

        [HttpGet("{cpf}/detalhes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorCpfAsync(string cpf)
        {

            var cliente = await _clienteService.ObterClientePorCpfAsync(cpf);
            if (cliente is null)
                return NotFound(new { message = "Cliente não encontrado." });

            return Ok(cliente);

        }

        [HttpGet]
        [Route("obterTodosClientes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterTodosClientes()
        {

            var clientes = await _clienteService.ObterTodosClientesAsync();

            if (clientes.Count == 0)
                return Ok(new { message = "Nenhum cliente encontrado no banco de dados." });


            return Ok(clientes);

        }

        [HttpPut("{id:guid}/atualizar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtualizarClienteRequest(Guid id, [FromBody] AtualizarClienteRequest request)
        {

            // Chama o serviço para atualizar o cliente
            var atualizado = await _clienteService.AtualizarClienteAsync(id, request);
            // Se o cliente não foi encontrado, retorna 404 Not Found
            if (!atualizado)
                return NotFound(new { message = "Cliente não encontrado." });

            return NoContent();

        }

        // altera somente o status do cliente
        [HttpPut("{id:guid}/ativar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtivarCliente(Guid id)
        {
            var clienteEncontrado = await _clienteService.AtivarClienteAsync(id);

            if (!clienteEncontrado)
                return NotFound(new { message = "Cliente não encontrado." });

            return NoContent();
        }

        // altera somente o status do cliente para inativo
        [HttpPut("{id:guid}/inativar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InativarCliente(Guid id)
        {
            var clienteEncontrado = await _clienteService.InativarClienteAsync(id);


            return NoContent();
        }


    }
}
