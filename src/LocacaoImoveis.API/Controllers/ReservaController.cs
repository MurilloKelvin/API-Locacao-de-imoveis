using LocacaoImoveis.Application.DTOs.Reservas;
using LocacaoImoveis.Application.Exceptions;
using LocacaoImoveis.Application.Services;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace LocacaoImoveis.API.Controllers
{
    /// <summary>Operações HTTP de reservas.</summary>
    [ApiController]
    [Route("api/v1/reservas")]
    public class ReservaController : ControllerBase
    {
        private readonly ReservaService _reservaService;

        /// <summary>Inicializa o controller com o serviço de aplicação.</summary>
        /// <param name="reservaService">Serviço responsável pelas operações do recurso.</param>
        public ReservaController(ReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        /// <summary>Cria uma reserva após validar cliente, imóvel e disponibilidade.</summary>
        /// <param name="request">Cliente, imóvel, datas de entrada e saída e quantidade de hóspedes.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="201">Reserva criada; retorna o ID e o endereço de consulta.</response>
        /// <response code="400">Dados inválidos ou cliente/imóvel inexistente.</response>
        /// <response code="409">Conflito de período ou regra de negócio violada.</response>
        [HttpPost]
        [EndpointSummary("Cria uma reserva")]
        [EndpointDescription("Cria uma reserva validando cliente, imovel, periodo e conflitos de disponibilidade.")]
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

        /// <summary>Lista reservas com filtros e paginação.</summary>
        /// <param name="filtro">Cliente, imóvel, status, página e tamanho da página.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="200">Reservas da página solicitada; lista vazia quando não há resultados.</response>
        /// <response code="400">Filtros ou paginação inválidos.</response>
        [HttpGet]
        [EndpointSummary("Lista reservas com filtros e paginação")]
        [EndpointDescription("Filtra por cliente, imóvel e status. Página padrão 1, tamanho padrão 10 e máximo 100. Ordena por data de entrada e ID.")]
        [ProducesResponseType(typeof(List<ReservaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObterTodasReservasAsync([FromQuery] ListarReservasRequest filtro, CancellationToken cancellationToken = default)
        {
            var reservas = await _reservaService.ObterTodasReservasAsync(filtro, cancellationToken);
            return Ok(reservas);
        }

        /// <summary>Consulta uma reserva pelo ID.</summary>
        /// <param name="id">Identificador da reserva.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="200">Dados da reserva, incluindo valores e status.</response>
        /// <response code="404">Reserva não encontrada.</response>
        [HttpGet]
        [Route("{id:guid}/detalhes")]
        [EndpointSummary("Busca uma reserva pelo ID")]
        [EndpointDescription("Retorna os dados da reserva identificada pelo GUID informado.")]
        [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterReservaPorIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var reserva = await _reservaService.ObterReservaPorIdAsync(id, cancellationToken);

            return Ok(reserva);
        }

        /// <summary>Cancela uma reserva e persiste o reembolso calculado.</summary>
        /// <param name="id">Identificador da reserva.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="204">Reserva cancelada; consulte os detalhes para obter o reembolso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="404">Reserva não encontrada.</response>
        /// <response code="409">O status atual não permite esta operação.</response>
        [HttpPut]
        [Route("{id:guid}/cancelar")]
        [EndpointSummary("Cancela uma reserva")]
        [EndpointDescription("Cancela a reserva e calcula o valor de reembolso conforme a politica vigente.")]
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

        /// <summary>Confirma uma reserva pendente.</summary>
        /// <param name="id">Identificador da reserva.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="204">Reserva confirmada.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="404">Reserva não encontrada.</response>
        /// <response code="409">O status atual não permite esta operação.</response>
        [HttpPut]
        [Route("{id:guid}/confirmar")]
        [EndpointSummary("Confirma uma reserva")]
        [EndpointDescription("Altera o status da reserva para confirmada.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ConfirmarReservaAsync(Guid id, CancellationToken cancellationToken = default)
        {

            await _reservaService.ConfirmarReservaAsync(id, cancellationToken);

            return NoContent();

        }

        /// <summary>Conclui uma reserva confirmada.</summary>
        /// <param name="id">Identificador da reserva.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="204">Reserva concluída.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="404">Reserva não encontrada.</response>
        /// <response code="409">O status atual não permite esta operação.</response>
        [HttpPut]
        [Route("{id:guid}/concluir")]
        [EndpointSummary("Conclui uma reserva")]
        [EndpointDescription("Altera o status da reserva para concluida.")]
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
