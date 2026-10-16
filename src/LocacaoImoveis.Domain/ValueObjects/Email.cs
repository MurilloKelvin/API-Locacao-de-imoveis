using System.Net.Mail;

namespace LocacaoImoveis.Domain.ValueObjects
{
    // sealed porque não queremos que seja herdado, e record porque é um Value Object
    public sealed record Email
    {
        public string Valor { get; } // n muda

        public Email(string valor)
        {
            // Validação básica do email nulo 
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("Email não pode ser vazio.", nameof(valor));

            //deixa o email em minúsculo e sem espaços no início e no fim
            Valor = valor.Trim().ToLowerInvariant();

            //se nao for possível interpretar como um endereço de email válido barra
            if (!MailAddress.TryCreate(Valor, out var endereço)
                || endereço.Address != Valor)
            { 
                throw new ArgumentException("Email inválido.", nameof(valor));
            }
        }
    }
}
