using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Domain.ValueObjects
{
    public sealed record DocumentoCliente
    {
        public string Valor { get; }


        //serve pra identificar se e cpf ou cnpj e ai cria o objeto correto
        public DocumentoCliente(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("Documento não pode ser vazio.", nameof(valor));

            int quantidadeCaracteres = valor.Count(char.IsLetterOrDigit);
                
            
            Valor = quantidadeCaracteres switch
            {
                11 => new Cpf(valor).Valor,
                14 => new Cnpj(valor).Valor,
                _ => throw new ArgumentException("Documento inválido. Deve ser CPF (11 dígitos) ou CNPJ (14 dígitos).", nameof(valor))
            };
        }
    }
}
