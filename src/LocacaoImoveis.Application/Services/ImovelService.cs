using LocacaoImoveis.Application.DTOs;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;

namespace LocacaoImoveis.Application.Services
{
    public class ImovelService
    {
        private readonly IImovelRepository _imovelRepository;

        public ImovelService(IImovelRepository imovelRepository)
        {
            _imovelRepository = imovelRepository;
        }

        public async Task<Guid> CriarImovelAsync(CriarImovelRequest request,
            CancellationToken cancellationToken = default)
        {
            var imovel = new Imovel(
                request.Endereco,
                request.Titulo,
                request.CapacidadeDeHospedes,
                request.ValorDiaria
            );

            await _imovelRepository.AdicionarAsync(imovel, cancellationToken);
            return imovel.Id;
        }

        public async Task<Imovel?> ObterImovelPorIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _imovelRepository.ObterImovelPorIdAsync(id, cancellationToken);
        }
    }
}
