using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.Application.Exceptions;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;


namespace LocacaoImoveis.Application.Services
{
    public class ImovelService
    {
        private readonly IImovelRepository _imovelRepository;
        private readonly IReservaRepository _reservaRepository;

        public ImovelService(IImovelRepository imovelRepository, IReservaRepository reservaRepository)
        {
            _imovelRepository = imovelRepository;
            _reservaRepository = reservaRepository;
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

        public async Task<ImovelResponse> ObterImovelPorIdAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            // Obter o imóvel do repositório
            var imovel = await _imovelRepository.ObterImovelPorIdAsync(id, cancellationToken);

            if (imovel == null)
                throw new RecursoNaoEncontradoException("Imóvel", id);

            // retorna o DTO de resposta
            return new ImovelResponse
            {
                Id = imovel.Id,
                Endereco = imovel.Endereco,
                Titulo = imovel.Titulo,
                CapacidadeHospedes = imovel.CapacidadeDeHospedes,
                ValorDiaria = imovel.ValorDiaria,
                Status = imovel.Status
            };
        }

        public async Task<List<ImovelResponse>> ListarImoveisAsync(
            ListarImoveisRequest filtro,
            CancellationToken cancellationToken = default)
        {

            // Validações dos filtros
            if (filtro.Pagina < 1)
                throw new ArgumentException("O número da página deve ser maior que 0.",nameof(filtro.Pagina));

            if(filtro.TamanhoPagina < 1 || filtro.TamanhoPagina > 100) 
                throw new ArgumentException("O tamanho da página deve estar entre 1 e 100.",nameof(filtro.TamanhoPagina));

            if(filtro.CapacidadeMinima <= 0 || filtro.CapacidadeMaxima <= 0)
                throw new ArgumentException("A capacidade máxima deve ser maior que 0.", nameof(filtro.CapacidadeMaxima));

            if(filtro.ValorDiarioMinimo <= 0 || filtro.ValorDiarioMaximo <= 0)
                throw new ArgumentException("O valor da diária deve ser maior que 0.", nameof(filtro.ValorDiarioMaximo));

            if(filtro.ValorDiarioMinimo > filtro.ValorDiarioMaximo)
                throw new ArgumentException("O valor da diária mínima deve ser menor que o valor da diária máxima.", nameof(filtro.ValorDiarioMaximo));

            // chamada pra buscar os imoveis com filtro
            var imoveis = await _imovelRepository.ListarImoveisAsync(filtro, cancellationToken);

            // busca os imóveis do repositório e mapeia para o DTO de resposta em lista
            return imoveis.Select(imovel => new ImovelResponse
            {
                Id = imovel.Id,
                Endereco = imovel.Endereco,
                Titulo = imovel.Titulo,
                CapacidadeHospedes = imovel.CapacidadeDeHospedes,
                ValorDiaria = imovel.ValorDiaria,
                Status = imovel.Status
            }).ToList();
        }

        public async Task AtualizarImovelAsync(Guid id, AtualizarImovelRequest request,
            CancellationToken cancellationToken = default)
        {
            var imovel = await _imovelRepository.ObterImovelPorIdAsync(id, cancellationToken);

            if (imovel is null)
                throw new RecursoNaoEncontradoException("Imóvel", id);

            imovel.AtualizarImovel(
                request.Endereco,
                request.Titulo,
                request.CapacidadeDeHospedes,
                request.ValorDiaria
            );

            await _imovelRepository.AtualizarImovelAsync(cancellationToken);
        }

        public async Task AlterarStatusImovelAsync(Guid id, AlterarStatusImovelRequest request,
            CancellationToken cancellationToken = default)
        { 
            if (!request.Status.HasValue) // verifica se o status foi informado
                throw new ArgumentException("O status do imovel deve ser informado.", nameof(request.Status));

            // busca o imovel pelo id
            var imovel = await _imovelRepository.ObterImovelPorIdAsync(id, cancellationToken);

            if (imovel is null)
                throw new RecursoNaoEncontradoException("Imóvel", id);

            if(request.Status == ImovelStatus.Indisponivel) // verifica se o status é indisponível
            {
                var reservasAtivas = await _reservaRepository.ContarReservasAtivas(id, cancellationToken);
                if (reservasAtivas > 0) // verifica se existem reservas ativas
                    throw new InvalidOperationException("Não é possível alterar o status do imóvel para indisponível, pois existem reservas ativas.");

            }

            imovel.AlterarStatusImovel(request.Status.Value); // altera o status 

                await _imovelRepository.AtualizarImovelAsync(cancellationToken); 
            
            }


            
            

    }
}