using LocacaoImoveis.Domain.Entities;

namespace LocacaoImoveis.Application.Interfaces
{
    public interface IClienteRepository
    {
        Task AdicionarClienteAsync(Cliente cliente);
        Task<Cliente?> ObterPorIdAsync(Guid id);

        Task<Cliente?> ObterPorCpfAsync(string cpf);

    }
}
