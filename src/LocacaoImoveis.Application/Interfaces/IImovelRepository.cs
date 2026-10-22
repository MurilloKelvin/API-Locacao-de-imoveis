using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Application.DTOs.Imoveis;

namespace LocacaoImoveis.Application.Interfaces
{
    public interface IImovelRepository
    {
        Task AdicionarAsync(Imovel imovel,
            CancellationToken cancellationToken = default);

        Task<Imovel?> ObterImovelPorIdAsync(Guid id,
            CancellationToken cancellationToken = default);

        Task<List<Imovel>> ListarImoveisAsync(ListarImoveisRequest filtro,
            CancellationToken cancellationToken = default);

        Task AtualizarImovelAsync(CancellationToken cancellationToken = default);

        Task<List<Imovel>> ListarImoveisDisponiveisAsync(ConsultarDisponibilidade request,
            CancellationToken cancellationToken = default);

    }
}
