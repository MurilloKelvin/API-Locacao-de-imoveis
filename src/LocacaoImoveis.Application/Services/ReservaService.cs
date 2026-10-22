using LocacaoImoveis.Application.DTOs.Reservas;
using LocacaoImoveis.Application.Exceptions;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;
using LocacaoImoveis.Domain.Policies;
using Microsoft.Extensions.Logging;

namespace LocacaoImoveis.Application.Services
{
    public class ReservaService
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IImovelRepository _imovelRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly PoliticaDeReembolso _politicaDeReembolso;
        private readonly ILogger<ReservaService> _logger;

        // lock para evitar duas threads ao mesmo tempo (1,1) 1 vaga de 1 maximo
        private static readonly SemaphoreSlim CriacaoReservaLock = new(1, 1);

        public ReservaService(
            IReservaRepository reservaRepository,
            IImovelRepository imovelRepository,
            IClienteRepository clienteRepository,
            PoliticaDeReembolso politicaDeReembolso,
            ILogger<ReservaService> logger)
        {
            _reservaRepository = reservaRepository;
            _imovelRepository = imovelRepository;
            _clienteRepository = clienteRepository;
            _politicaDeReembolso = politicaDeReembolso;
            _logger = logger;
        }

        public async Task<Guid> CriarReservaAsync(
            CriarReservaRequest request,
            CancellationToken cancellationToken = default)
        {
            //log
            _logger.LogInformation("Criando rserva para o cliente {ClienteId} e imóvel {ImovelId}",
                request.ClienteId, request.ImovelId);



            // lock pra evitar duas threads ao mesmo tempo
            await CriacaoReservaLock.WaitAsync(cancellationToken);
            try
            {
                //busca o cliente para verificar se ele existe e se está ativo
                var cliente = await _clienteRepository.ObterPorIdAsync(request.ClienteId);

                if (cliente is null)
                    throw new ArgumentException("Cliente não encontrado", nameof(request.ClienteId));

                var contadorReservasCliente = await _reservaRepository.ContarReservasAtivasPorCliente(request.ClienteId, cancellationToken);

                if (contadorReservasCliente >= 3)
                    throw new InvalidOperationException("Cliente não pode ter mais de 3 reservas ativas");

                if (cliente.Status != ClienteStatus.Ativo)
                    throw new InvalidOperationException("Cliente inativo não pode realizar reservas");

                //busca o imovel para verificar se ele existe e se está disponível
                var imovel = await _imovelRepository.ObterImovelPorIdAsync(request.ImovelId);

                if (imovel is null)
                    throw new ArgumentException("Imóvel não encontrado", nameof(request.ImovelId));

                if (imovel.Status != ImovelStatus.Disponivel)
                    throw new InvalidOperationException("Imóvel não está disponível para reserva");

                if (request.QuantidadeHospedes > imovel.CapacidadeDeHospedes)
                    throw new InvalidOperationException("Quantidade de hóspedes excede a capacidade do imóvel");

                // verifica se ja tem reservas/conflitos para o imovel
                var existeConflito = await _reservaRepository.ExisteConflitos(request.ImovelId, request.DataEntrada, request.DataSaida, cancellationToken);

                if (existeConflito)
                    throw new InvalidOperationException("Já existe uma reserva para este imóvel no período informado");

                // verifica o numero de dias
                var quantidadeDeNoites = (request.DataSaida.DayNumber - request.DataEntrada.DayNumber);
                // calcula o valor total da reserva
                var valorTotal = quantidadeDeNoites * imovel.ValorDiaria;

                //cria a reserva
                var reserva = new Reserva(
                    request.ClienteId,
                    request.ImovelId,
                    request.DataEntrada,
                    request.DataSaida,
                    request.QuantidadeHospedes,
                    valorTotal,
                    DateTimeOffset.UtcNow
                    );

                //salva a reserva
                await _reservaRepository.AdicionarAsync(reserva, cancellationToken);

                //log
                    _logger.LogInformation("reserva criada parao cliente {ClienteId} e imóvel {ImovelId}",
                    request.ClienteId, request.ImovelId);
                
                return reserva.Id;
            }

            // esse finnaly diz pra liberar o lock podendo criar outra reserva
            finally
            {
                CriacaoReservaLock.Release(); 
            }

        }

        public async Task<List<ReservaResponse>> ObterTodasReservasAsync()
        {
            var reservas = await _reservaRepository.ObterTodasReservasAsync();

            // mapeia as reservas para o DTO de resposta
            return reservas.Select(r => new ReservaResponse
            {
                Id = r.Id,
                ClienteId = r.ClienteId,
                ImovelId = r.ImovelId,
                QuantidadeDeNoites = r.CalcularQuantidadeNoites(),
                DataEntrada = r.DataEntrada,
                DataSaida = r.DataSaida,
                QuantidadeHospedes = r.QuantidadeHospedes,
                ValorTotalPreco = r.ValorTotalPreco,
                Status = r.Status,
                CriadaEm = r.CriadaEm,
                CanceladaEm = r.CanceladaEm,
                ValorReembolso = r.ValorReembolso
            }).ToList();
        }

        public async Task<ReservaResponse> ObterReservaPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObterIdAsync(id, cancellationToken);

            if (reserva is null)
                throw new RecursoNaoEncontradoException("Reserva", id);

            return new ReservaResponse
            {
                Id = reserva.Id,
                ClienteId = reserva.ClienteId,
                ImovelId = reserva.ImovelId,
                QuantidadeDeNoites = reserva.CalcularQuantidadeNoites(),
                DataEntrada = reserva.DataEntrada,
                DataSaida = reserva.DataSaida,
                QuantidadeHospedes = reserva.QuantidadeHospedes,
                ValorTotalPreco = reserva.ValorTotalPreco,
                Status = reserva.Status,
                CriadaEm = reserva.CriadaEm,
                CanceladaEm = reserva.CanceladaEm,
                ValorReembolso = reserva.ValorReembolso
            };
        }

        public async Task ConcluirReservaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObterIdAsync(id, cancellationToken);
            if (reserva is null)
                throw new RecursoNaoEncontradoException("Reserva", id);

            reserva.Concluir();

            await _reservaRepository.AtualizarAsync(cancellationToken);

            //log
            _logger.LogInformation("Reserva concluida para o cliente {ClienteId} e imovel {ImovelId}",
                reserva.ClienteId, reserva.ImovelId);
        }

        public async Task ConfirmarReservaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObterIdAsync(id, cancellationToken);
            if (reserva is null)
                throw new RecursoNaoEncontradoException("Reserva", id);

            reserva.Confirmar();

            await _reservaRepository.AtualizarAsync(cancellationToken);

            //log
            _logger.LogInformation("Reserva confirmada para o cliente {ClienteId} e imovel {ImovelId}",
                reserva.ClienteId, reserva.ImovelId);
        }

        public async Task CancelarReservaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObterIdAsync(id, cancellationToken);
            if (reserva is null)
                throw new RecursoNaoEncontradoException("Reserva", id);

            var fuso = TimeSpan.FromHours(-3); // fuso horario de brasília

            var cancelamento = DateTimeOffset.UtcNow.ToOffset(fuso); // pega a data e hora atual no fuso horario de brasília


            // pega a data e hora de entrada da reserva no fuso horario de brasília
            var dataHoraEntrada = reserva.DataEntrada.ToDateTime(
                new TimeOnly(14, 0),
                DateTimeKind.Unspecified);

            // cria um DateTimeOffset com a data e hora de entrada da reserva no fuso horario de brasília
            var entrada = new DateTimeOffset(dataHoraEntrada, fuso);

            // calcula o reembolso considerando o horario de 14h do dia de entrada da reserva, e a data e hora atual no fuso horario de brasília
            var valorReembolso = _politicaDeReembolso.CalcularReembolso(reserva.ValorTotalPreco,
                entrada,
                cancelamento
                 );

            reserva.Cancelar(
                cancelamento,
                valorReembolso);

            await _reservaRepository.AtualizarAsync(cancellationToken);

            //log
            _logger.LogInformation("Reserva cancelada para o cliente {ClienteId} e imovel {ImovelId}",
                reserva.ClienteId, reserva.ImovelId);
        }

    }







}
