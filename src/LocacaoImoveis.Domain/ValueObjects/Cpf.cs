namespace LocacaoImoveis.Domain.ValueObjects
{
    public sealed record Cpf
    {
        public string Valor { get; }

        public Cpf(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("CPF não pode ser vazio.", nameof(valor));

            // Normaliza o CPF removendo pontos e traços e espaços em vazios
            var CpfNormalizado = valor.Trim()
                .Replace(".", "")
                .Replace("-", "");

            // ve se ls caracteres são todos números 0-9 (unicode) e usa ValidaCPF 
            if (CpfNormalizado.Any(caractere => caractere < '0' || caractere > '9')
                || !ValidaCPF.IsCpf(CpfNormalizado))
                throw new ArgumentException("CPF deve conter apenas números.", nameof(valor));

            Valor = CpfNormalizado;
        }
    }
}
    