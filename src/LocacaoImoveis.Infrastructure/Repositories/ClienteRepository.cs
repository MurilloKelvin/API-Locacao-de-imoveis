using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;

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
    }
}
