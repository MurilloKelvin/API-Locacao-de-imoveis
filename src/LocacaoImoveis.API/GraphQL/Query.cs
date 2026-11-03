using HotChocolate.Authorization;
using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.Application.DTOs.Reservas;
using LocacaoImoveis.Application.Services;

namespace LocacaoImoveis.API.GraphQL;

public class Query
{
    // Clientes
    // passa o id e usa o service para buscar o cliente
    [Authorize]
    public Task<ClienteResponse> Cliente(
        Guid id,
        ClienteService service)
        => service.ObterClientePorIdAsync(id);

    [Authorize]
    public Task<ClienteResponse> ClientePorDocumento(
        string documento,
        ClienteService service)
        => service.ObterClientePorDocumentoAsync(documento);

    [Authorize]
    public Task<List<ClienteResponse>> Clientes(
        ListarClientesRequest filtro,
        ClienteService service,
        CancellationToken cancellationToken)
        => service.ObterTodosClientesAsync(filtro, cancellationToken);

    // Reservas
    [Authorize]
    public Task<ReservaResponse> Reserva(
        Guid id,
        ReservaService service,
        CancellationToken cancellationToken)
        => service.ObterReservaPorIdAsync(id, cancellationToken);

    [Authorize]
    public Task<List<ReservaResponse>> Reservas(
        ListarReservasRequest filtro,
        ReservaService service,
        CancellationToken cancellationToken)
        => service.ObterTodasReservasAsync(filtro, cancellationToken);

    // Imóveis
    public Task<ImovelResponse> Imovel(
        Guid id,
        ImovelService service,
        CancellationToken cancellationToken)
        => service.ObterImovelPorIdAsync(id, cancellationToken);

    public Task<List<ImovelResponse>> Imoveis(
        ListarImoveisRequest filtro,
        ImovelService service,
        CancellationToken cancellationToken)
        => service.ListarImoveisAsync(filtro, cancellationToken);

    public Task<List<ImovelResponse>> ImoveisDisponiveis(
        ConsultarDisponibilidade filtro,
        ImovelService service,
        CancellationToken cancellationToken)
        => service.ListarImoveisDisponiveisAsync(filtro, cancellationToken);
}