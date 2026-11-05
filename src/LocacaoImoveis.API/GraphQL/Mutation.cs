using HotChocolate.Authorization;
using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.API.Auth;
using Microsoft.Extensions.Options;
using LocacaoImoveis.Application.DTOs.Reservas;
using LocacaoImoveis.Application.Services;

namespace LocacaoImoveis.API.GraphQL;

/// <summary>Operações de escrita disponíveis no endpoint GraphQL.</summary>
public class Mutation
{
    // auth
    //publica
    /// <summary>Valida as credenciais e retorna um token para uso no GraphQL.</summary>
    /// <param name="clientId">Identificador do cliente da API.</param>
    /// <param name="clientSecret">Segredo do cliente da API.</param>
    /// <param name="tokenService">Serviço de emissão de tokens.</param>
    /// <param name="options">Configurações utilizadas pela operação.</param>
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
    /// <summary>Cadastra um cliente.</summary>
    /// <param name="input">Dados da operação.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    [Authorize]
    public Task<Guid> CriarCliente(
        CriarClienteRequestDTO input,
        ClienteService service,
        CancellationToken cancellationToken)
        => service.CriarClienteAsync(input, cancellationToken);

    /// <summary>Atualiza os dados de um cliente.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="input">Dados da operação.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    [Authorize]
    public async Task<bool> AtualizarCliente(
        Guid id,
        AtualizarClienteRequest input,
        ClienteService service)
    {
        await service.AtualizarClienteAsync(id, input);
        return true;
    }

    /// <summary>Ativa um cliente.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    [Authorize]
    public async Task<bool> AtivarCliente(Guid id, ClienteService service)
    {
        await service.AtivarClienteAsync(id);
        return true;
    }

    /// <summary>Inativa um cliente.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    [Authorize]
    public async Task<bool> InativarCliente(Guid id, ClienteService service)
    {
        await service.InativarClienteAsync(id);
        return true;
    }

    // Reservas
    /// <summary>Cria uma reserva.</summary>
    /// <param name="input">Dados da operação.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    [Authorize]
    public Task<Guid> CriarReserva(
        CriarReservaRequest input,
        ReservaService service,
        CancellationToken cancellationToken)
        => service.CriarReservaAsync(input, cancellationToken);

    /// <summary>Confirma uma reserva pendente.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    [Authorize]
    public async Task<bool> ConfirmarReserva(
        Guid id,
        ReservaService service,
        CancellationToken cancellationToken)
    {
        await service.ConfirmarReservaAsync(id, cancellationToken);
        return true;
    }

    /// <summary>Conclui uma reserva confirmada.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    [Authorize]
    public async Task<bool> ConcluirReserva(
        Guid id,
        ReservaService service,
        CancellationToken cancellationToken)
    {
        await service.ConcluirReservaAsync(id, cancellationToken);
        return true;
    }

    /// <summary>Cancela uma reserva e registra o reembolso.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
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
    /// <summary>Cadastra um imóvel.</summary>
    /// <param name="input">Dados da operação.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public Task<Guid> CriarImovel(
        CriarImovelRequest input,
        ImovelService service,
        CancellationToken cancellationToken)
        => service.CriarImovelAsync(input, cancellationToken);

    /// <summary>Atualiza os dados de um imóvel.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="input">Dados da operação.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public async Task<bool> AtualizarImovel(
        Guid id,
        AtualizarImovelRequest input,
        ImovelService service,
        CancellationToken cancellationToken)
    {
        await service.AtualizarImovelAsync(id, input, cancellationToken);
        return true;
    }

    /// <summary>Altera o status de um imóvel.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="input">Dados da operação.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
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
/// <summary>Token de acesso e data de expiração retornados pelo login.</summary>
public record LoginResponse(string AccessToken, DateTime ExpiraEm);