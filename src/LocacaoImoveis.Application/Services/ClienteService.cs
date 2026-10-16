using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Application.DTOs;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;

namespace LocacaoImoveis.Application.Services
{
    public class ClienteService
    {
        private readonly IClienteRepository _clienteRepository; // dependencia

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<Guid> CriarClienteAsync(CriarClienteRequestDTO request)
        {
            
            var cliente = new Cliente(
                request.Nome,
                request.Telefone,
                request.Email,
                request.Cpf
                );

            await _clienteRepository.AdicionarClienteAsync(cliente); // salva o cliente no repositório

            return cliente.Id;
        }
    }
}
