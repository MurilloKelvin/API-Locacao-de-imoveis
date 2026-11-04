using LocacaoImoveis.API.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LocacaoImoveis.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/auth")]
public class AuthController(TokenService tokenService, IOptions<JwtOptions> options) : ControllerBase
{
    public record TokenRequest(string ClientId, string ClientSecret);

    [HttpPost("token")]
    public IActionResult Token(TokenRequest request)
    {
        var jwt = options.Value;

        // Confere as credenciais enviadas com as configuradas no user secrets
        // Se estiverem erradas, responde 401 (não autorizado)
        if (request.ClientId != jwt.ClientId || request.ClientSecret != jwt.ClientSecret)
            return Unauthorized();

        var (token, expiraEm) = tokenService.GerarToken(jwt.ClientId);
        return Ok(new { accessToken = token, expiraEm });
    }
}