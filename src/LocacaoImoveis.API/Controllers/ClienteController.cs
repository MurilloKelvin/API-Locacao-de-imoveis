using Microsoft.AspNetCore.Mvc;
using LocacaoImoveis.Application.DTOs;
using LocacaoImoveis.Application.Services;  

namespace LocacaoImoveis.API.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    public class ClienteController : ControllerBase
    {

        private readonly ClienteService _clienteService; // dependência do serviço de cliente
        public ClienteController(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)] // Indica que a resposta será 201 Created
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Indica que a resposta será 400 Bad Request
        public async Task<IActionResult> CriarCliente(
            [FromBody] CriarClienteRequestDTO request)
        {
            try
            {
                var clienteId = await _clienteService.CriarClienteAsync(request);

                return StatusCode(StatusCodes.Status201Created, new { id = clienteId }); 
            }
            catch (ArgumentException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Indica que a resposta será 200 OK
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Indica que a resposta será 404 Not Found
        public async Task<IActionResult> ObterClientePorId(Guid id)
        {
            var cliente = await _clienteService.ObterClientePorIdAsync(id);
            if (cliente == null)
                return NotFound(new { message = "Cliente não encontrado." });

            return Ok(cliente);
        }

        [HttpGet("cpf/{cpf}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorCpfAsync(string cpf)
        {
            try
            {
                var cliente = await _clienteService.ObterClientePorCpfAsync(cpf);
                if (cliente is null)
                    return NotFound(new { message = "Cliente não encontrado." });

                return Ok(cliente);

            }
            catch (ArgumentException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

    }
}
