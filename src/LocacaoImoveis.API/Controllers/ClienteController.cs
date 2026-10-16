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
    }
}
