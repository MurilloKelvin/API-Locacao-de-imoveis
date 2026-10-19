using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Entities;

namespace LocacaoImoveis.Application.Interfaces
{
    public interface IImovelRepository
    {
        Task AdicionarAsync(Imovel imovel,
            CancellationToken cancellationToken = default);
    }
}
