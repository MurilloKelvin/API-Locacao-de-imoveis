using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.DTOs.Reservas;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.Services
{
    public class ReservaService
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IImovelRepository _imovelRepository;
        private readonly IClienteRepository _clienteRepository;

        public ReservaService(IReservaRepository reservaRepository, IImovelRepository imovelRepository, IClienteRepository clienteRepository)
        {
            _reservaRepository = reservaRepository;
            _imovelRepository = imovelRepository;
            _clienteRepository = clienteRepository;
        }

        public async Task<Guid> CriarReservaAsync(
            CriarReservaRequest request,
            CancellationToken cancellationToken = default)
         {
            //busca o cliente para verificar se ele existe e se está ativo
            var cliente = await _clienteRepository.ObterPorIdAsync(request.ClienteId);

            if (cliente is null)
                throw new ArgumentException("Cliente não encontrado", nameof(request.ClienteId));

            var contadorReservasCliente = await _reservaRepository.ContarReservasAtivasPorCliente(request.ClienteId, cancellationToken); 

            if(contadorReservasCliente >= 3)
                throw new InvalidOperationException("Cliente não pode ter mais de 3 reservas ativas");

            if(cliente.Status != ClienteStatus.Ativo)
                throw new InvalidOperationException("Cliente inativo não pode realizar reservas");

            //busca o imovel para verificar se ele existe e se está disponível
            var imovel = await _imovelRepository.ObterImovelPorIdAsync(request.ImovelId);

            if(imovel is null)
                throw new ArgumentException("Imóvel não encontrado", nameof(request.ImovelId));

            if(imovel.Status != ImovelStatus.Disponivel)
                throw new InvalidOperationException("Imóvel não está disponível para reserva");

            if(request.QuantidadeHospedes > imovel.CapacidadeDeHospedes)
                throw new InvalidOperationException("Quantidade de hóspedes excede a capacidade do imóvel");

            // verifica se ja tem reservas/conflitos para o imovel
            var existeConflito = await _reservaRepository.ExisteConflitos(request.ImovelId, request.DataEntrada, request.DataSaida, cancellationToken);

            if(existeConflito)
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
            
            return reserva.Id;
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

        public async Task<ReservaResponse?> ObterReservaPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObterIdAsync(id, cancellationToken);

            if (reserva is null)
                return null;

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

        public async Task<bool> ConcluirReservaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObterIdAsync(id, cancellationToken);
            if (reserva is null)
                return false;

            reserva.Concluir();

            await _reservaRepository.AtualizarAsync(cancellationToken);
            return true;

        }

        public async Task<bool> ConfirmarReservaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObterIdAsync(id, cancellationToken);
            if (reserva is null)
                return false;

            reserva.Confirmar();

            await _reservaRepository.AtualizarAsync(cancellationToken);
            return true;

        }

        public async Task<bool> CancelarReservaAsync(Guid id, decimal valorReembolso, CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObterIdAsync(id, cancellationToken);
            if (reserva is null)
                return false;

            reserva.Cancelar(
                DateTimeOffset.UtcNow,
                valorReembolso);

            await _reservaRepository.AtualizarAsync(cancellationToken);
            return true;

        }

    }







}
