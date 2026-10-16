using System;
using System.Linq;

public static class ValidaCPF
{
    public static bool IsCpf(string cpf)
    {

        // Verifica tamanho e dígitos repetidos
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
            return false;

        int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;

        // Primeiro dígito
        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        int digito1 = (resto < 2) ? 0 : 11 - resto;

        // Segundo dígito
        tempCpf += digito1;
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        int digito2 = (resto < 2) ? 0 : 11 - resto;

        return cpf.EndsWith($"{digito1}{digito2}");
    }
}