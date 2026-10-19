using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using LocacaoImoveis.Application.DTOs.Imoveis;

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

        public async Task<Imovel?> ObterImovelPorIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            // busca imovel pelo ID 
            return await _context.Imoveis.FindAsync(new object[] { id }, cancellationToken); 
        }

        public async Task<List<Imovel>> ListarImoveisAsync(
            ListarImoveisRequest filtro,
            CancellationToken cancellationToken = default)
        {
            var consulta = _context.Imoveis.AsNoTracking();

            // ve se o filtro foi informado, caso sim, o where e aplicado na consulta passando o valor do filtro
            if (filtro.Status.HasValue)
                consulta = consulta.Where(i => i.Status == filtro.Status.Value);

            if (filtro.CapacidadeMinima.HasValue)
                consulta = consulta.Where(i =>
                    i.CapacidadeDeHospedes >= filtro.CapacidadeMinima.Value);

            if (filtro.CapacidadeMaxima.HasValue)
                consulta = consulta.Where(i =>
                    i.CapacidadeDeHospedes <= filtro.CapacidadeMaxima.Value);

            if (filtro.ValorDiarioMinimo.HasValue)
                consulta = consulta.Where(i =>
                    i.ValorDiaria >= filtro.ValorDiarioMinimo.Value);

            if (filtro.ValorDiarioMaximo.HasValue)
                consulta = consulta.Where(i =>
                    i.ValorDiaria <= filtro.ValorDiarioMaximo.Value);

            return await consulta
                .OrderBy(i => i.Titulo)
                .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
                .Take(filtro.TamanhoPagina)
                .ToListAsync(cancellationToken);
        }

        public async Task AtualizarImovelAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
