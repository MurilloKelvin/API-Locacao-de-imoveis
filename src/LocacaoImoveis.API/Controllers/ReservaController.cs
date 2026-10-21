using LocacaoImoveis.Application.DTOs.Reservas;
using LocacaoImoveis.Application.Exceptions;
using LocacaoImoveis.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocacaoImoveis.API.Controllers
{
    [ApiController]
    [Route("api/v1/reservas")]
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
            // chamando o serviço para criar a reserva
            var reservaId = await _reservaService.CriarReservaAsync(request, cancellationToken);

            //retorna o status 201 Created com o id da reserva criada 
            return Created(
                $"/api/v1/reservas/{reservaId}/detalhes",
                new { id = reservaId });

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterTodasReservasAsync(CancellationToken cancellationToken = default)
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

            return Ok(reserva);
        }

        [HttpPut]
        [Route("{id:guid}/cancelar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CancelarReservaAsync(
            Guid id,
             CancellationToken cancellationToken = default)
        {

            // chamando o serviço para cancelar a reserva
            await _reservaService.CancelarReservaAsync(id, cancellationToken);

            return NoContent();

        }

        [HttpPut]
        [Route("{id:guid}/confirmar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ConfirmarReservaAsync(Guid id, CancellationToken cancellationToken = default)
        {

            await _reservaService.ConfirmarReservaAsync(id, cancellationToken);

            return NoContent();

        }

        [HttpPut]
        [Route("{id:guid}/concluir")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ConcluirReservaAsync(Guid id, CancellationToken cancellationToken = default)
        {

            await _reservaService.ConcluirReservaAsync(id, cancellationToken);

            return NoContent();

        }


    }
}
