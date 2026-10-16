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

            var clienteExiste = await _clienteRepository.ObterPorCpfAsync(request.Cpf);
            if(clienteExiste != null)
                throw new Exception("Cliente com este CPF já existe.");

            await _clienteRepository.AdicionarClienteAsync(cliente); // salva o cliente no repositório

            return cliente.Id;
        }

        public async Task<ClienteResponse?> ObterClientePorIdAsync(Guid id)
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(id);
            if (cliente == null)   
                return null;
            
            return new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Email = cliente.Email.Valor,
                Cpf = cliente.Cpf.Valor
            };
        }

        public async Task<ClienteResponse?> ObterClientePorCpfAsync(string cpf)
        {
            var cliente = await _clienteRepository.ObterPorCpfAsync(cpf);
            if (cliente == null)
                return null;
            return new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Email = cliente.Email.Valor,
                Cpf = cliente.Cpf.Valor
            };
        }

    }
}
