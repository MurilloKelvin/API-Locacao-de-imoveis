using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using LocacaoImoveis.Application.Exceptions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;


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
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateException ex)
                when (ex.InnerException is SqliteException sqlite // so executo se der a exceçao de unicidade 
                && sqlite.SqliteExtendedErrorCode == 2067)
            { 
                throw new CpfJaCadastradoException(); 
            }
        }

        public async Task<Cliente?> ObterPorIdAsync(Guid id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<Cliente?> ObterPorEmailAsync(string email)
        {
            return await _context.Clientes.FirstOrDefaultAsync(c => c.Email.Valor == email);
        }

        public async Task<Cliente?> ObterPorCpfAsync(string cpf)
        {
            var cpfValidado = new Cpf(cpf);

           return await _context.Clientes.FirstOrDefaultAsync(c => c.Cpf == cpfValidado);
        }

        public async Task<List<Cliente>> ObterTodosClientesAsync()
        {
            return await _context.Clientes
                .OrderBy(Cliente => Cliente.Nome) // traz a lista ordenada pelo nome do cliente
                .ToListAsync();
        }

        public async Task AtualizarClienteAsync(Cliente cliente)
        {
            await _context.SaveChangesAsync();
        }

    }
}
