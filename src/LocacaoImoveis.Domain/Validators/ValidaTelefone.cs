using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LocacaoImoveis.Domain.Validators
{
    public static class ValidaTelefone
    {

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
