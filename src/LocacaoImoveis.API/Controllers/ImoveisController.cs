using Microsoft.AspNetCore.Mvc;
using LocacaoImoveis.Application.Services;
using LocacaoImoveis.Application.DTOs;


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

    }
}
