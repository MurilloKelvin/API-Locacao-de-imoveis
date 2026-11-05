using HotChocolate.Authorization;
using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.Application.DTOs.Reservas;
using LocacaoImoveis.Application.Services;

namespace LocacaoImoveis.API.GraphQL;

/// <summary>Consultas disponíveis no endpoint GraphQL.</summary>
public class Query
{
    // Clientes
    // passa o id e usa o service para buscar o cliente
    /// <summary>Consulta um cliente pelo identificador.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    [Authorize]
    public Task<ClienteResponse> Cliente(
        Guid id,
        ClienteService service)
        => service.ObterClientePorIdAsync(id);

    /// <summary>Consulta um cliente pelo CPF ou CNPJ.</summary>
    /// <param name="documento">CPF ou CNPJ do cliente.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    [Authorize]
    public Task<ClienteResponse> ClientePorDocumento(
        string documento,
        ClienteService service)
        => service.ObterClientePorDocumentoAsync(documento);

    /// <summary>Lista clientes conforme os filtros informados.</summary>
    /// <param name="filtro">Filtros e paginação da consulta.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    [Authorize]
    public Task<List<ClienteResponse>> Clientes(
        ListarClientesRequest filtro,
        ClienteService service,
        CancellationToken cancellationToken)
        => service.ObterTodosClientesAsync(filtro, cancellationToken);

    // Reservas
    /// <summary>Consulta uma reserva pelo identificador.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    [Authorize]
    public Task<ReservaResponse> Reserva(
        Guid id,
        ReservaService service,
        CancellationToken cancellationToken)
        => service.ObterReservaPorIdAsync(id, cancellationToken);

    /// <summary>Lista reservas conforme os filtros informados.</summary>
    /// <param name="filtro">Filtros e paginação da consulta.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    [Authorize]
    public Task<List<ReservaResponse>> Reservas(
        ListarReservasRequest filtro,
        ReservaService service,
        CancellationToken cancellationToken)
        => service.ObterTodasReservasAsync(filtro, cancellationToken);

    // Imóveis
    /// <summary>Consulta um imóvel pelo identificador.</summary>
    /// <param name="id">Identificador do recurso.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public Task<ImovelResponse> Imovel(
        Guid id,
        ImovelService service,
        CancellationToken cancellationToken)
        => service.ObterImovelPorIdAsync(id, cancellationToken);

    /// <summary>Lista imóveis conforme os filtros informados.</summary>
    /// <param name="filtro">Filtros e paginação da consulta.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public Task<List<ImovelResponse>> Imoveis(
        ListarImoveisRequest filtro,
        ImovelService service,
        CancellationToken cancellationToken)
        => service.ListarImoveisAsync(filtro, cancellationToken);

    /// <summary>Lista imóveis disponíveis para o período solicitado.</summary>
    /// <param name="filtro">Filtros e paginação da consulta.</param>
    /// <param name="service">Serviço de aplicação responsável pela operação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public Task<List<ImovelResponse>> ImoveisDisponiveis(
        ConsultarDisponibilidade filtro,
        ImovelService service,
        CancellationToken cancellationToken)
        => service.ListarImoveisDisponiveisAsync(filtro, cancellationToken);
}