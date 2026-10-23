using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.Application.Services;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;


namespace LocacaoImoveis.API.Controllers
{
    /// <summary>Operações HTTP de imóveis.</summary>
    [ApiController]
    [Route("api/v1/imoveis")]
    public class ImoveisController : ControllerBase
    {
        private readonly ImovelService _imovelService;

        /// <summary>Inicializa o controller com o serviço de aplicação.</summary>
        /// <param name="imovelService">Serviço responsável pelas operações do recurso.</param>
        public ImoveisController(ImovelService imovelService)
        {
            _imovelService = imovelService;
        }

        /// <summary>Cadastra um imóvel disponível para reserva.</summary>
        /// <param name="request">Título, endereço, capacidade e valor da diária.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="201">Imóvel criado; retorna o ID e o endereço de consulta.</response>
        /// <response code="400">Dados inválidos.</response>
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

        /// <summary>Consulta um imóvel pelo ID.</summary>
        /// <param name="id">Identificador do imóvel.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="200">Dados do imóvel.</response>
        /// <response code="404">Imóvel não encontrado.</response>
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

        /// <summary>Lista imóveis com filtros e paginação.</summary>
        /// <param name="filtro">Status, capacidade, faixa de preço e paginação.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="200">Imóveis da página solicitada.</response>
        /// <response code="400">Filtros ou paginação inválidos.</response>
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

        /// <summary>Consulta imóveis disponíveis para um período e quantidade de hóspedes.</summary>
        /// <param name="request">Período da estadia, hóspedes, faixa de preço e paginação.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="200">Imóveis disponíveis; lista vazia quando não há resultados.</response>
        /// <response code="400">Parâmetros de consulta inválidos.</response>
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




        /// <summary>Atualiza os dados de um imóvel.</summary>
        /// <param name="id">Identificador do imóvel.</param>
        /// <param name="request">Título, endereço, capacidade e valor da diária.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="204">Imóvel atualizado.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="404">Imóvel não encontrado.</response>
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

        /// <summary>Altera o status de um imóvel respeitando as reservas ativas.</summary>
        /// <param name="id">Identificador do imóvel.</param>
        /// <param name="request">Novo status do imóvel.</param>
        /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
        /// <response code="204">Status atualizado.</response>
        /// <response code="400">Status inválido.</response>
        /// <response code="404">Imóvel não encontrado.</response>
        /// <response code="409">Alteração incompatível com as regras do imóvel ou reservas ativas.</response>
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
