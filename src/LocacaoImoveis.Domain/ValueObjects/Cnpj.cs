using System.Text.RegularExpressions;

namespace LocacaoImoveis.Domain.ValueObjects
{
    public sealed record Cnpj
    {
        public string Valor { get; }

        public Cnpj(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("CNPJ não pode ser vazio.", nameof(valor));



            // Normaliza o CNPJ removendo pontos e traços e espaços em vazios
            var CnpjNormalizado = valor.Trim()
                .Replace(".", "")
                .Replace("-", "")
                .Replace("/", "")
                .ToUpperInvariant();

            //verifica se o CNPJ tem 14 caracteres e se os 12 primeiros são letras ou números e os 2 últimos são números
            if (!Regex.IsMatch(CnpjNormalizado, @"\A[A-Z0-9]{12}[0-9]{2}\z")) // 
                throw new ArgumentException("CNPJ inválido.", nameof(valor));

            Valor = CnpjNormalizado;
        }
    }
}
