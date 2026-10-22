using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.Application.Services;
using Microsoft.AspNetCore.Http.Metadata;
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
        [EndpointSummary("Cadastra um novo imovel")]
        [EndpointDescription("Cria um imovel com os dados informados.")]
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
        [EndpointSummary("Busca um imovel pelo ID")]
        [EndpointDescription("Retorna os dados do imovel identificado pelo GUID informado.")]
        [ProducesResponseType(typeof(ImovelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterImovelPorId(Guid id, CancellationToken cancellationToken)
        {
            var imovelResponse = await _imovelService.ObterImovelPorIdAsync(id, cancellationToken);

            return Ok(imovelResponse);
        }

        [HttpGet]
        [Route("listar")]
        [EndpointSummary("Lista imoveis")]
        [EndpointDescription("Lista imoveis aplicando os filtros e a paginacao informados.")]
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

        [HttpGet("imoveisDisponiveis")]
        [EndpointSummary("Lista imoveis disponiveis")]
        [EndpointDescription("Retorna imoveis disponiveis para o periodo, capacidade e faixa de preco informados.")]
        [ProducesResponseType(typeof(List<ImovelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ListarImoveisDisponiveis(
            [FromQuery] ConsultarDisponibilidade request,
            CancellationToken cancellationToken = default)
        {
            // list aos imoveis disponíveis para o cliente com o id fornecido
            var imoveisDisponiveis = await _imovelService.ListarImoveisDisponiveisAsync(request, cancellationToken);

            return Ok(imoveisDisponiveis);
        }




        [HttpPut("{id:guid}/atualizarImovel")]
        [EndpointSummary("Atualiza um imovel")]
        [EndpointDescription("Atualiza os dados do imovel identificado pelo GUID informado.")]
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
        [EndpointSummary("Altera o status de um imovel")]
        [EndpointDescription("Altera o status do imovel respeitando as regras de negocio.")]
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
