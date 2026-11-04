using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LocacaoImoveis.API.Auth;

public sealed class TokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _jwt = options.Value;

    public (string Token, DateTime ExpiraEm) GerarToken(string clientId)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_jwt.ExpiracaoMinutos);
        // transforma o texto da chave em byte para assinatura
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = expiraEm,
            // infos guardadas no token, que podem ser lidas pelo destinatário do token (audience)
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, clientId),
                new Claim(ClaimTypes.Role, "api-client")
            ]),
            // assina o token com a secret key
            SigningCredentials = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256)
        };
        //retorna o token gerado e a data de expiração
        return (new JsonWebTokenHandler().CreateToken(descriptor), expiraEm);
    }
}