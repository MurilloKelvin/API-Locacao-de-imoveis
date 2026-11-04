namespace LocacaoImoveis.API.Auth;

// o .net preenche automaticamente as propriedades do objeto com os valores da seção Jwt (appsettings + user secrets)
public sealed class JwtOptions
{
    // emite o token
    public string Issuer { get; init; } = string.Empty;
    // para quem o token é emitido
    public string Audience { get; init; } = string.Empty;
    // chave secreta usada para assinar o token
    public string Key { get; init; } = string.Empty;

    public int ExpiracaoMinutos { get; init; } = 50;
    // credenciais
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
}