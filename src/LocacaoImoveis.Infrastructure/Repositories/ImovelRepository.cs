using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.Domain.Enums;

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

        public async Task<List<Imovel>> ListarImoveisDisponiveisAsync(
            ConsultarDisponibilidade request,
            CancellationToken cancellationToken = default)
        {
            // busca todos os imoveis disponiveis
            var consulta = _context.Imoveis.AsNoTracking()
                .Where(i => i.Status == ImovelStatus.Disponivel && i.CapacidadeDeHospedes >= request.QuantidadeHospedes);

            // ve se o filtro foi informado, caso sim, o where e aplicado na consulta passando o valor do filtro
            if(request.ValorDiarioMaximo.HasValue)
                consulta = consulta.Where(i =>
                    i.ValorDiaria <= request.ValorDiarioMaximo.Value);

            if (request.ValorDiarioMinimo.HasValue)
                consulta = consulta.Where(i =>
                    i.ValorDiaria >= request.ValorDiarioMinimo.Value);

            // filtra os imoveis que não possuem reservas pendentes ou confirmadas no período informado e nao conflitam com a data de entrada e saida informadas
            consulta = consulta.Where(imovel =>
                !_context.Reservas.Any(reserva =>
                    reserva.ImovelId == imovel.Id &&
                    (reserva.Status == ReservaStatus.Pendente ||
                     reserva.Status == ReservaStatus.Confirmada) &&
                    reserva.DataEntrada < request.Saida &&
                    reserva.DataSaida > request.Entrada));

            // retorna a lista com os imoveis dispniveis orndenando pelo titulo
            return await consulta
                .OrderBy(i => i.Titulo)
                .ThenBy(i => i.Id)
                .Skip((request.Paginas - 1) * request.ItensPorPagina)
                .Take(request.ItensPorPagina)
                .ToListAsync(cancellationToken);
        }

    }
}
