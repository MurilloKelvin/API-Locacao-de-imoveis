using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Application.DTOs.Reservas;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;

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

            if(cliente is null)
                throw new ArgumentException("Cliente não encontrado",nameof(request.ClienteId));

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



    }






    
    }
