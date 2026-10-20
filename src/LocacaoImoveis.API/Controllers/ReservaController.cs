using Microsoft.AspNetCore.Mvc;
using LocacaoImoveis.Application.Services;
using LocacaoImoveis.Application.DTOs.Reservas;

namespace LocacaoImoveis.API.Controllers
{
    [ApiController]
    [Route("api/reservas")]
    public class ReservaController : ControllerBase
    {
        private readonly ReservaService _reservaService;

        public ReservaController(ReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CriarReservaAsync(
            [FromBody] CriarReservaRequest request, CancellationToken cancellationToken)
        {
            try
            {   // chamando o serviço para criar a reserva
                var reservaId = await _reservaService.CriarReservaAsync(request, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, new { Id = reservaId });
            }
            catch (ArgumentException ex)
            { 
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterTodasReservasAsync()
        {
            var reservas = await _reservaService.ObterTodasReservasAsync();
            return Ok(reservas);
        }


    }
}
