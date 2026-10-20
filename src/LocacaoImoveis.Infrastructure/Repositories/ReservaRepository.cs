using LocacaoImoveis.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using LocacaoImoveis.Domain.Enums;
using System.Security.Cryptography.X509Certificates;

namespace LocacaoImoveis.Infrastructure.Repositories
{
    public class ReservaRepository : IReservaRepository
    {
        private readonly AppDbContext _context;

        public ReservaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Reserva reserva, CancellationToken cancellationToken = default)
        {
            await _context.Reservas.AddAsync(reserva, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Reserva?> ObterIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Reservas.FindAsync(new object[] { id }, cancellationToken);

        }

        public async Task AtualizarAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> ContarReservasAtivas(
            Guid imovelId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Reservas
                .CountAsync(r => r.ImovelId == imovelId && //verifica se a reserva e do mesmo imovel e se ta como pendente ou confirmdo
                                (r.Status == ReservaStatus.Pendente ||
                                 r.Status == ReservaStatus.Confirmada),
                           cancellationToken);
        }

        public async Task<bool> ExisteConflitos(
            Guid imovelId,
            DateOnly entrada,
            DateOnly saida,
            CancellationToken cancellationToken = default)
        {
            {
                //procura pelomenos uma reserva que atende os requisitos: mesmo imovel
                //somente reservas pendentes ou confirmadas bloqueiam datas
                // verifica se as datas se cruzam
                return await _context.Reservas.AnyAsync(r => r.ImovelId == imovelId &&
                                                        r.Status == ReservaStatus.Pendente ||
                                                       (r.Status == ReservaStatus.Confirmada) &&

                                                        r.DataEntrada < saida &&
                                                        r.DataSaida > entrada, cancellationToken);
            }
        }


    }
}
