using Microsoft.AspNetCore.Mvc;
using LocacaoImoveis.Application.Services;
using LocacaoImoveis.Application.DTOs.Imoveis;


namespace LocacaoImoveis.API.Controllers
{
    [ApiController]
    [Route("api/imoveis")]
    public class ImoveisController : ControllerBase
    {
        private readonly ImovelService _imovelService;

        public ImoveisController(ImovelService imovelService)
        {
            _imovelService = imovelService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CriarImovel([FromBody] CriarImovelRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var imovelId = await _imovelService.CriarImovelAsync(request, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, new { Id = imovelId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ImovelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterImovelPorId(Guid id, CancellationToken cancellationToken)
        {
            var imovelResponse = await _imovelService.ObterImovelPorIdAsync(id, cancellationToken);

            if (imovelResponse == null)
                return NotFound("Imóvel não encontrado.");

            return Ok(imovelResponse);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ImovelResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarImoveis(
            [FromQuery] ListarImoveisRequest filtro,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // list aos imoveis com filtro agora
                var imoveis = await _imovelService.ListarImoveisAsync(filtro, cancellationToken);
                return Ok(imoveis);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtualizarImovel(Guid id,
            [FromBody] AtualizarImovelRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // atualizar o imóvel com o id fornecido
                var imovelAtualizado = await _imovelService.AtualizarImovelAsync(id, request, cancellationToken);

                if (!imovelAtualizado)
                    return NotFound("Imóvel não encontrado.");
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AlterarStatusImovel(Guid id,
            [FromBody] AlterarStatusImovelRequest request, CancellationToken cancellationToken = default)
        {

            try
            {
                // alterar o status do imóvel com o id fornecido
                var atualizado = await _imovelService.AlterarStatusImovelAsync(id, request, cancellationToken);

                if(!atualizado)
                    return NotFound(new { message = "Imóvel não encontrado." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
    }
}
