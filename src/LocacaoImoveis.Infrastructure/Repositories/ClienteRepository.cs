using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace LocacaoImoveis.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _context; // injeção de dependência do contexto do banco de dados

        public ClienteRepository(AppDbContext context) // construtor que recebe o contexto do banco de dados
        {
            _context = context;
        }

        public async Task AdicionarClienteAsync(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task<Cliente?> ObterPorIdAsync(Guid id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<Cliente?> ObterPorCpfAsync(string cpf)
        {
            var cpfValidado = new Cpf(cpf);

           return await _context.Clientes.FirstOrDefaultAsync(c => c.Cpf == cpfValidado);
        }

    }
}
