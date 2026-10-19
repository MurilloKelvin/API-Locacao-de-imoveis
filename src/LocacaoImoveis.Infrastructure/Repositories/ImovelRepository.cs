using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Application.Interfaces;

namespace LocacaoImoveis.Infrastructure.Repositories
{
    public class ImovelRepository : IImovelRepository
    {
        private readonly AppDbContext _context;

        public ImovelRepository(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task AdicionarAsync(Imovel imovel,
            CancellationToken cancellationToken = default)
        {
            await _context.Imoveis.AddAsync(imovel, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
