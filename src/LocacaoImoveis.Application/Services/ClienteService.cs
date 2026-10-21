using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Application.Exceptions;
using LocacaoImoveis.Application.DTOs.Clientes;

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

            var emailClienteExiste = await _clienteRepository.ObterPorEmailAsync(request.Email);
            var cpfClienteExiste = await _clienteRepository.ObterPorCpfAsync(request.Cpf);

            if(emailClienteExiste != null)
                throw new EmailJaCadastradoException();

            if(cpfClienteExiste != null)
                throw new CpfJaCadastradoException();



            await _clienteRepository.AdicionarClienteAsync(cliente); // salva o cliente no repositório

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

        public async Task<List<ClienteResponse>> ObterTodosClientesAsync()
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

            cliente.AtualizarDadosCliente(request.Nome, request.Telefone, request.Email);
            await _clienteRepository.AtualizarClienteAsync(cliente);
        }

        public async Task AtivarClienteAsync(Guid id)
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(id);
            if (cliente == null)
                throw new RecursoNaoEncontradoException("Cliente", id);

            cliente.AtivarCliente();

            await _clienteRepository.AtualizarClienteAsync(cliente);
        }

        public async Task InativarClienteAsync(Guid id)
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(id); 
            if(cliente == null)
                throw new RecursoNaoEncontradoException("Cliente", id);

            cliente.InativarCliente();

            await _clienteRepository.AtualizarClienteAsync(cliente);
        }


        }
}
