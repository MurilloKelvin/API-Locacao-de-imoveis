using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.Application.Exceptions;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LocacaoImoveis.Application.Services
{
    public class ImovelService
    {
        private readonly IImovelRepository _imovelRepository;
        private readonly IReservaRepository _reservaRepository;

        private readonly ILogger<ImovelService> _logger;

        public ImovelService(IImovelRepository imovelRepository, IReservaRepository reservaRepository, ILogger<ImovelService> logger)
        {
            _imovelRepository = imovelRepository;
            _reservaRepository = reservaRepository;
            _logger = logger;
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

            //log
            _logger.LogInformation("Imóvel criado com sucesso. Id: {Id}, Endereço: {Endereco}, Título: {Titulo}, Capacidade: {Capacidade}, Valor Diária: {ValorDiaria}",
                imovel.Id, imovel.Endereco, imovel.Titulo, imovel.CapacidadeDeHospedes, imovel.ValorDiaria);

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

        public async Task<List<ImovelResponse>> ListarImoveisDisponiveisAsync(
            ConsultarDisponibilidade request,
            CancellationToken cancellationToken = default)
        {
            // Validações dos filtros
            var hoje = DateOnly.FromDateTime(
                DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3)).DateTime);

            //se o valor foi null atribui a data padrao
            request.Entrada ??= hoje;
            request.Saida ??= request.Entrada.Value.AddDays(30);


            var entrada = request.Entrada.Value;
            var saida = request.Saida.Value;


            if (entrada < hoje)
                throw new ArgumentException("A entrada não pode ser no passado.");

            if (saida <= request.Entrada)
                throw new ArgumentException("A saída deve ser posterior à entrada.");

            if (saida.DayNumber - entrada.DayNumber > 30)
                throw new ArgumentException("A estadia não pode superar 30 noites.");

            if (request.QuantidadeHospedes <= 0)
                throw new ArgumentException(
                    "A quantidade de hóspedes deve ser maior que zero.");

            if (request.Paginas < 1)
                throw new ArgumentException("A página deve ser maior que zero.");

            if (request.ItensPorPagina < 1 || request.ItensPorPagina > 100)
                throw new ArgumentException(
                    "A quantidade de itens por página deve estar entre 1 e 100.");

            if (request.ValorDiarioMinimo < 0 || request.ValorDiarioMaximo < 0)
                throw new ArgumentException("Os preços não podem ser negativos.");

            if (request.ValorDiarioMinimo > request.ValorDiarioMaximo)
                throw new ArgumentException(
                    "O preço mínimo não pode superar o máximo.");

            //busca os imoveis com os filtros
            var imoveis = await _imovelRepository.ListarImoveisDisponiveisAsync(
                request, cancellationToken);

            // retorna a lista de imovesm mapeados em DTOs de resposta
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

            //log
            _logger.LogInformation("Imóvel atualizado com sucesso. Id: {Id}, Endereço: {Endereco}, Título: {Titulo}, Capacidade: {Capacidade}, Valor Diária: {ValorDiaria}",
                imovel.Id, imovel.Endereco, imovel.Titulo, imovel.CapacidadeDeHospedes, imovel.ValorDiaria);
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


            // se for indisponível ou em manutenção, verifica se há reservas ativas, caso sim lança exceçao barrando a alteração de status
            if (request.Status == ImovelStatus.Indisponivel ||
                request.Status == ImovelStatus.EmManutencao)
            {
                var reservasAtivas = await _reservaRepository
                    .ContarReservasAtivas(id, cancellationToken);

                if (reservasAtivas > 0)
                    throw new InvalidOperationException(
                        "Não é possível deixar o imóvel indisponível ou em manutenção "
                        + "enquanto houver reservas ativas.");
            }

            imovel.AlterarStatusImovel(request.Status.Value); // altera o status 

                await _imovelRepository.AtualizarImovelAsync(cancellationToken); 

            _logger.LogInformation("Status do imóvel alterado com sucesso. Id: {Id}, Novo Status: {Status}",
                imovel.Id, imovel.Status);

        }


            
            

    }
}