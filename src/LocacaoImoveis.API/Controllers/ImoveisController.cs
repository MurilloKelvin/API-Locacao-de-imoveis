using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.Application.Services;
using Microsoft.AspNetCore.Mvc;


namespace LocacaoImoveis.API.Controllers
{
    [ApiController]
    [Route("api/v1/imoveis")]
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
            var imovelId = await _imovelService.CriarImovelAsync(request, cancellationToken);

            //retorna um objeto com o id do imóvel criado
            return CreatedAtAction(
                nameof(ObterImovelPorId),
                new { id = imovelId },
                new { id = imovelId });

        }

        [HttpGet("{id:guid}/detalhes")]
        [ProducesResponseType(typeof(ImovelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterImovelPorId(Guid id, CancellationToken cancellationToken)
        {
            var imovelResponse = await _imovelService.ObterImovelPorIdAsync(id, cancellationToken);

            return Ok(imovelResponse);
        }

        [HttpGet]
        [Route("listar")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(List<ImovelResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarImoveis(
            [FromQuery] ListarImoveisRequest filtro,
            CancellationToken cancellationToken = default)
        {

            // list aos imoveis com filtro agora
            var imoveis = await _imovelService.ListarImoveisAsync(filtro, cancellationToken);
            return Ok(imoveis);


        }

        [HttpPut("{id:guid}/atualizarImovel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtualizarImovel(Guid id,
            [FromBody] AtualizarImovelRequest request,
            CancellationToken cancellationToken = default)
        {

            // atualizar o imóvel com o id fornecido
            await _imovelService.AtualizarImovelAsync(id, request, cancellationToken);
            return NoContent();

        }

        [HttpPut("{id:guid}/atualizarStatus")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AlterarStatusImovel(Guid id,
            [FromBody] AlterarStatusImovelRequest request, CancellationToken cancellationToken = default)
        {

            // alterar o status do imóvel com o id fornecido
            await _imovelService.AlterarStatusImovelAsync(id, request, cancellationToken);

            return NoContent();

        }
    }
}
