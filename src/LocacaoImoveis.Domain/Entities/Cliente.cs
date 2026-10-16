using LocacaoImoveis.Domain.Enums;
using LocacaoImoveis.Domain.ValueObjects;

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
        public Cpf Cpf { get; private set; }

        // Construtor privado para EF 
        private Cliente()
        {
            Nome = null!;
            Telefone = null!;
            Email = null!;
            Cpf = null!;
        }

        public Cliente(string nome, string telefone, string email, string cpf)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome não pode ser vazio.", nameof(nome));

            Id = Guid.NewGuid();
            Nome = nome.Trim();
            Telefone = telefone;
            Status = ClienteStatus.Ativo;
            Email = new Email(email); // recebe o email e cria o Value Object Email
            Cpf = new Cpf(cpf);
        }
    } }
