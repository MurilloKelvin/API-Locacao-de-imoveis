using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LocacaoImoveis.Domain.Validators
{
    /// <summary>Valida números de telefone de clientes.</summary>
    public static class ValidaTelefone
    {

        /// <summary>Valida e retorna um telefone com 10 ou 11 dígitos.</summary>
        /// <param name="telefone">Telefone do cliente.</param>
        public static string ValidarTelefone(string telefone)
        {
            // Valida se o telefone é nulo ou vazio
            if (string.IsNullOrWhiteSpace(telefone))
                throw new ArgumentException("Telefone não pode ser vazio.", nameof(telefone));

            // valida se o telefone contém apenas dígitos e tem entre 10 e 11 dígitos 
            if (!Regex.IsMatch(telefone, @"^\d{10,11}$"))
                throw new ArgumentException("Telefone deve conter apenas dígitos.", nameof(telefone));

            var numeroTelefone = telefone.Trim();
            
            return numeroTelefone; 

        }

    }
}
