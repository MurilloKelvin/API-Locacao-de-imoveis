using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.Exceptions;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Domain.Entities;
using Microsoft.Extensions.Logging;
using LocacaoImoveis.Application.Services;

namespace LocacaoImoveis.Application.Services
{
    public class ClienteService
    {
        private readonly IClienteRepository _clienteRepository; // dependencia
        private readonly IReservaRepository _reservaRepository; // dependencia
        private readonly ILogger<ClienteService> _logger; 

        public ClienteService(IClienteRepository clienteRepository, ILogger<ClienteService> logger, IReservaRepository reservaRepository)
        {
            _clienteRepository = clienteRepository;
            _logger = logger;
            _reservaRepository = reservaRepository;
        }

        public async Task<Guid> CriarClienteAsync(CriarClienteRequestDTO request, CancellationToken cancellationToken = default)
        {
            
            var cliente = new Cliente(
                request.Nome,
                request.Telefone,
                request.Email,
                request.Documento
                );

            var emailClienteExiste = await _clienteRepository.ObterPorEmailAsync(request.Email);
            var DocumentoClienteExiste = await _clienteRepository.ObterPorDocumentoAsync(request.Documento);

            if(emailClienteExiste != null)
                throw new EmailJaCadastradoException();

            if(DocumentoClienteExiste != null)
                throw new CpfJaCadastradoException();

            await _clienteRepository.AdicionarClienteAsync(cliente); // salva o cliente no repositório

            //log
            _logger.LogInformation("Cliente criado com sucesso. Id: {Id}, Nome: {Nome}, Email: {Email}, Documento: {Documento}",
                cliente.Id, cliente.Nome, cliente.Email.Valor, cliente.DocumentoCliente.Valor);

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
                Documento = cliente.DocumentoCliente.Valor,
                Status = cliente.Status
            };
        }

        public async Task<ClienteResponse> ObterClientePorDocumentoAsync(string documento)
        {
            var cliente = await _clienteRepository.ObterPorDocumentoAsync(documento);
            if (cliente == null)
                throw new RecursoNaoEncontradoException("Cliente", documento);
            return new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Email = cliente.Email.Valor,
                Documento = cliente.DocumentoCliente.Valor,
                Status = cliente.Status

            };
        }

        public async Task<List<ClienteResponse>> ObterTodosClientesAsync(ListarClientesRequest filtro, CancellationToken cancellationToken = default)
        {
            if (filtro.Pagina < 1 || filtro.TamanhoPagina < 1 || filtro.TamanhoPagina > 100
                || ((long)filtro.Pagina - 1) * filtro.TamanhoPagina > int.MaxValue)
                throw new ArgumentException("Página inválida ou tamanho da página fora do intervalo de 1 a 100.");

            if (filtro.Status.HasValue && !Enum.IsDefined(filtro.Status.Value))
                throw new ArgumentException("Status de cliente inválido.");

            // busca os clientes no repositório com base nos filtros fornecidos
            var clientes = await _clienteRepository.ObterTodosClientesAsync(filtro, cancellationToken);
            return clientes.Select(cliente => new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Telefone = cliente.Telefone,
                Email = cliente.Email.Valor,
                Documento = cliente.DocumentoCliente.Valor,
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

            // Verifica se o cliente possui reservas ativas
            var imoveisAtivos = await _reservaRepository.ContarReservasAtivasPorCliente(id); 
            if (imoveisAtivos > 0)
                throw new InvalidOperationException("Não é possível inativar um cliente com reservas ativas.");

            cliente.InativarCliente();

            await _clienteRepository.AtualizarClienteAsync(cliente);

            //log
            _logger.LogInformation("Cliente inativado com sucesso. Id: {Id}, Nome: {Nome}",
                cliente.Id, cliente.Nome);
        }


    }
}
