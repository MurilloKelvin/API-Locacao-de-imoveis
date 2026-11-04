using HotChocolate.Authorization;
using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.API.Auth;
using Microsoft.Extensions.Options;
using LocacaoImoveis.Application.DTOs.Reservas;
using LocacaoImoveis.Application.Services;

namespace LocacaoImoveis.API.GraphQL;

public class Mutation
{
    // auth
    //publica
    [AllowAnonymous]
    public LoginResponse Login(
        string clientId,
        string clientSecret,
        TokenService tokenService,
        IOptions<JwtOptions> options)
    {
        var jwt = options.Value;
        // Confere as credenciais enviadas com as configuradas no user secrets
        // Se estiverem erradas, responde 401 (não autorizado)
        if (clientId != jwt.ClientId || clientSecret != jwt.ClientSecret)
            throw new GraphQLException("Credenciais inválidas.");
        var (token, expiraEm) = tokenService.GerarToken(jwt.ClientId);
        return new LoginResponse(token, expiraEm);
    }




    // Clientes
    [Authorize]
    public Task<Guid> CriarCliente(
        CriarClienteRequestDTO input,
        ClienteService service,
        CancellationToken cancellationToken)
        => service.CriarClienteAsync(input, cancellationToken);

    [Authorize]
    public async Task<bool> AtualizarCliente(
        Guid id,
        AtualizarClienteRequest input,
        ClienteService service)
    {
        await service.AtualizarClienteAsync(id, input);
        return true;
    }

    [Authorize]
    public async Task<bool> AtivarCliente(Guid id, ClienteService service)
    {
        await service.AtivarClienteAsync(id);
        return true;
    }

    [Authorize]
    public async Task<bool> InativarCliente(Guid id, ClienteService service)
    {
        await service.InativarClienteAsync(id);
        return true;
    }

    // Reservas
    [Authorize]
    public Task<Guid> CriarReserva(
        CriarReservaRequest input,
        ReservaService service,
        CancellationToken cancellationToken)
        => service.CriarReservaAsync(input, cancellationToken);

    [Authorize]
    public async Task<bool> ConfirmarReserva(
        Guid id,
        ReservaService service,
        CancellationToken cancellationToken)
    {
        await service.ConfirmarReservaAsync(id, cancellationToken);
        return true;
    }

    [Authorize]
    public async Task<bool> ConcluirReserva(
        Guid id,
        ReservaService service,
        CancellationToken cancellationToken)
    {
        await service.ConcluirReservaAsync(id, cancellationToken);
        return true;
    }

    [Authorize]
    public async Task<bool> CancelarReserva(
        Guid id,
        ReservaService service,
        CancellationToken cancellationToken)
    {
        await service.CancelarReservaAsync(id, cancellationToken);
        return true;
    }

    // Imóveis
    public Task<Guid> CriarImovel(
        CriarImovelRequest input,
        ImovelService service,
        CancellationToken cancellationToken)
        => service.CriarImovelAsync(input, cancellationToken);

    public async Task<bool> AtualizarImovel(
        Guid id,
        AtualizarImovelRequest input,
        ImovelService service,
        CancellationToken cancellationToken)
    {
        await service.AtualizarImovelAsync(id, input, cancellationToken);
        return true;
    }

    public async Task<bool> AlterarStatusImovel(
        Guid id,
        AlterarStatusImovelRequest input,
        ImovelService service,
        CancellationToken cancellationToken)
    {
        await service.AlterarStatusImovelAsync(id, input, cancellationToken);
        return true;
    }
}
// resposta do login, com o token e a data de expiração
public record LoginResponse(string AccessToken, DateTime ExpiraEm);