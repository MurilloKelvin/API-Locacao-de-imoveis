using LocacaoImoveis.Domain.Entities;

namespace LocacaoImoveis.Application.Interfaces
{
    public interface IClienteRepository
    {
        Task AdicionarClienteAsync(Cliente cliente);
    }
}
