using LocacaoImoveis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.Interfaces
{
    public interface IReservaRepository
    {
        Task AdicionarAsync(Reserva reserva, CancellationToken cancellationToken = default);

        Task<List<Reserva>> ObterTodasReservasAsync();

        Task<Reserva?> ObterIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task AtualizarAsync(CancellationToken cancellationToken = default);

        Task<bool> ExisteConflitos(Guid imovelId,
            DateOnly entrada,
            DateOnly saida,
            CancellationToken  cancellationToken = default);

        Task<int>ContarReservasAtivas(Guid id, CancellationToken cancelToken = default);
    }
}
