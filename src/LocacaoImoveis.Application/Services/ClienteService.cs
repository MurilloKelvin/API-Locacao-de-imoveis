using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.Exceptions;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LocacaoImoveis.Application.Services
{
    public class ClienteService
    {
        private readonly IClienteRepository _clienteRepository; // dependencia
        private readonly ILogger<ClienteService> _logger; 

        public ClienteService(IClienteRepository clienteRepository, ILogger<ClienteService> logger)
        {
            _clienteRepository = clienteRepository;
            _logger = logger;
        }

        public async Task<Guid> CriarClienteAsync(CriarClienteRequestDTO request, CancellationToken cancellationToken = default)
        {
            
            var cliente = new Cliente(
                request.Nome,
                request.Telefone,
                request.Email,
                request.Cpf
                );

            var emailClienteExiste = await _clienteRepository.ObterPorEmailAsync(request.Email);
            var cpfClienteExiste = await _clienteRepository.ObterPorCpfAsync(request.Cpf);

            if(emailClienteExiste != null)
                throw new EmailJaCadastradoException();

            if(cpfClienteExiste != null)
                throw new CpfJaCadastradoException();

            await _clienteRepository.AdicionarClienteAsync(cliente); // salva o cliente no repositório

            //log
            _logger.LogInformation("Cliente criado com sucesso. Id: {Id}, Nome: {Nome}, Email: {Email}, Cpf: {Cpf}",
                cliente.Id, cliente.Nome, cliente.Email.Valor, cliente.Cpf.Valor);

            return cliente.Id;
        }

        public async Task<ClienteResponse> ObterClientePorIdAsync(Guid id)
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(id);
            if (cliente == null)   
                throw new RecursoNaoEncontradoException("Cliente", id);
            
            return new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Email = cliente.Email.Valor,
                Cpf = cliente.Cpf.Valor,
                Status = cliente.Status
            };
        }

        public async Task<ClienteResponse> ObterClientePorCpfAsync(string cpf)
        {
            var cliente = await _clienteRepository.ObterPorCpfAsync(cpf);
            if (cliente == null)
                throw new RecursoNaoEncontradoException("Cliente", cpf);
            return new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Email = cliente.Email.Valor,
                Cpf = cliente.Cpf.Valor,
                Status = cliente.Status

            };
        }

        public async Task<List<ClienteResponse>> ObterTodosClientesAsync(CancellationToken cancellationToken = default)
        {
            var clientes = await _clienteRepository.ObterTodosClientesAsync();
            return clientes.Select(cliente => new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Email = cliente.Email.Valor,
                Cpf = cliente.Cpf.Valor,
                Status = cliente.Status
            }).ToList();
        }

        public async Task AtualizarClienteAsync(Guid id, AtualizarClienteRequest request)
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(id);
            if (cliente == null)
                throw new RecursoNaoEncontradoException("Cliente", id);

            cliente.AtualizarDadosCliente(request.Nome, request.Telefone);

            await _clienteRepository.AtualizarClienteAsync(cliente);

            //log
            _logger.LogInformation("Cliente atualizado com sucesso. Id: {Id}, Nome: {Nome}, Telefone: {Telefone}",
                cliente.Id, cliente.Nome, cliente.Telefone);
        }

        public async Task AtivarClienteAsync(Guid id)
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(id);
            if (cliente == null)
                throw new RecursoNaoEncontradoException("Cliente", id);

            cliente.AtivarCliente();

            await _clienteRepository.AtualizarClienteAsync(cliente);

            //log
            _logger.LogInformation("Cliente ativado com sucesso. Id: {Id}, Nome: {Nome}",
                cliente.Id, cliente.Nome);
        }

        public async Task InativarClienteAsync(Guid id)
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(id); 
            if(cliente == null)
                throw new RecursoNaoEncontradoException("Cliente", id);

            cliente.InativarCliente();

            await _clienteRepository.AtualizarClienteAsync(cliente);

            //log
            _logger.LogInformation("Cliente inativado com sucesso. Id: {Id}, Nome: {Nome}",
                cliente.Id, cliente.Nome);
        }


    }
}
