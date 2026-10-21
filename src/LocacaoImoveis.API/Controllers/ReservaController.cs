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

        [HttpGet]
        [Route("{id:guid}/detalhes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterReservaPorIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var reserva = await _reservaService.ObterReservaPorIdAsync(id, cancellationToken);

            if (reserva is null)
                return NotFound(new { message = "Reserva não encontrada" });

            return Ok(reserva);
        }

        [HttpPut]
        [Route("{id:guid}/cancelar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CancelarReservaAsync(Guid id,
            [FromBody] CancelarReservaRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // chamando o serviço para cancelar a reserva
                var cancelada = await _reservaService.CancelarReservaAsync(id, request.ValorReembolso, cancellationToken);

                if (!cancelada)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new {message = ex.Message});
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("{id:guid}/confirmar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ConfirmarReservaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var confirmada = await _reservaService.ConfirmarReservaAsync(id, cancellationToken);

                if(!confirmada)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("{id:guid}/concluir")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ConcluirReservaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var concluida = await _reservaService.ConcluirReservaAsync(id, cancellationToken);

                if (!concluida)
                    return NotFound();

                return NoContent(); 
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }


    }
}
