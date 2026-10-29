using LocacaoImoveis.Domain.Enums;
using LocacaoImoveis.Domain.ValueObjects;
using LocacaoImoveis.Domain.Validators;

namespace LocacaoImoveis.Domain.Entities
{
    public class Cliente
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Telefone { get; private set; }
        public ClienteStatus Status { get; private set; }

        // Obj
        public Email Email { get; private set; }
        public DocumentoCliente DocumentoCliente { get; private set; }
        // Construtor privado para EF 
        private Cliente()
        {
            Nome = null!;
            Telefone = null!;
            Email = null!;
            DocumentoCliente = null!;
        }

        public Cliente(string nome, string telefone, string email, string documento)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome não pode ser vazio.", nameof(nome));

            Id = Guid.NewGuid();
            Nome = nome.Trim();
            Telefone = ValidaTelefone.ValidarTelefone(telefone); // valida usando o validator
            Status = ClienteStatus.Ativo;
            Email = new Email(email); // recebe o email e cria o Value Object Email
            DocumentoCliente = new DocumentoCliente(documento);
        }

        public void AtualizarDadosCliente(string nome, string telefone)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome não pode ser vazio.", nameof(nome));


            Nome = nome.Trim();

            var telefoneValidado = ValidaTelefone.ValidarTelefone(telefone); // valida usando o validator

            Telefone = telefoneValidado;

        }

        public void AtivarCliente()
        {
            Status = ClienteStatus.Ativo;
        }

        public void InativarCliente()
        {
            Status = ClienteStatus.Inativo;
        }

    } }
