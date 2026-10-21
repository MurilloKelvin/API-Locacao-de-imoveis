using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using LocacaoImoveis.Application.DTOs.Reservas;
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
            // Conta somente reservas do imóvel informado.
            // Reservas pendentes e confirmadas são consideradas ativas.
            // Reservas concluídas ou canceladas não bloqueiam o imóvel.
            return await _context.Reservas.CountAsync(
                reserva =>
                    reserva.ImovelId == imovelId &&
                    (reserva.Status == ReservaStatus.Pendente ||
                     reserva.Status == ReservaStatus.Confirmada),
                cancellationToken);
        }

        public async Task<int> ContarReservasAtivasPorCliente(
            Guid clienteId,
            CancellationToken cancellationToken = default)
        {
            // Conta somente reservas do cliente informado.
            // Reservas pendentes e confirmadas são consideradas ativas.
            // Reservas concluídas ou canceladas não bloqueiam o imóvel.
            return await _context.Reservas.CountAsync(
                reserva =>
                    reserva.ClienteId == clienteId &&
                    (reserva.Status == ReservaStatus.Pendente ||
                     reserva.Status == ReservaStatus.Confirmada),
                cancellationToken);
        }

        public async Task<bool> ExisteConflitos(
            Guid imovelId,
            DateOnly entrada,
            DateOnly saida,
            CancellationToken cancellationToken = default)
        {
            // Procura pelo menos uma reserva que possa bloquear o período solicitado.
            return await _context.Reservas.AnyAsync(
                reserva =>
                    // A reserva precisa pertencer ao imóvel consultado.
                    reserva.ImovelId == imovelId &&

                    // Somente reservas pendentes ou confirmadas bloqueiam o período.
                    (reserva.Status == ReservaStatus.Pendente ||
                     reserva.Status == ReservaStatus.Confirmada) &&

                    // Os períodos se sobrepõem quando a entrada ocorre antes da saída
                    // existente e a saída ocorre depois da entrada existente.
                    reserva.DataEntrada < saida &&
                    reserva.DataSaida > entrada,
                cancellationToken);
        }

        public async Task<List<Reserva>> ObterTodasReservasAsync()
        {
            var reservas = await _context.Reservas.ToListAsync();

            // Ordena as reservas pela data de criação (CriadaEm) antes de retornar
            return reservas
                .OrderBy(reserva => reserva.CriadaEm)
                .ToList();
        }



    }
}
