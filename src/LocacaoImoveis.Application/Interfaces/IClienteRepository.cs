using LocacaoImoveis.Domain.Entities;

using LocacaoImoveis.Application.DTOs.Clientes;

namespace LocacaoImoveis.Application.Interfaces
{
    public interface IClienteRepository
    {
        Task AdicionarClienteAsync(Cliente cliente, CancellationToken cancellationToken = default);
        Task<Cliente?> ObterPorIdAsync(Guid id);
        Task<Cliente?> ObterPorDocumentoAsync(string documento);
        Task<Cliente?> ObterPorEmailAsync(string email);
        Task<List<Cliente>> ObterTodosClientesAsync(ListarClientesRequest filtro, CancellationToken cancellationToken = default);

        Task AtualizarClienteAsync(Cliente cliente, CancellationToken cancellationToken = default);

    }
}
