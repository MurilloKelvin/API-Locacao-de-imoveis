namespace LocacaoImoveis.API.Auth;

// o .net preenche automaticamente as propriedades do objeto com os valores da seção Jwt (appsettings + user secrets)
/// <summary>Configurações de emissão e validação dos tokens JWT.</summary>
public sealed class JwtOptions
{
    // emite o token
    /// <summary>Emissor dos tokens.</summary>
    public string Issuer { get; init; } = string.Empty;
    // para quem o token é emitido
    /// <summary>Destinatário dos tokens.</summary>
    public string Audience { get; init; } = string.Empty;
    // chave secreta usada para assinar o token
    /// <summary>Chave usada para assinar os tokens.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Validade do token em minutos.</summary>
    public int ExpiracaoMinutos { get; init; } = 50;
    // credenciais
    /// <summary>Identificador do cliente da API.</summary>
    public string ClientId { get; init; } = string.Empty;
    /// <summary>Segredo do cliente da API.</summary>
    public string ClientSecret { get; init; } = string.Empty;
}