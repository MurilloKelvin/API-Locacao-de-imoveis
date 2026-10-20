using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Domain.Policies
{
    public class PoliticaDeReembolso
    {

        public decimal CalcularReembolso(decimal valorTotal, DateTimeOffset entrada, DateTimeOffset cancelamento)
        {
            if(valorTotal < 0)
                throw new ArgumentException("O valor total deve ser maior que zero", nameof(valorTotal));

            var diasAntesDaEntrada = (entrada - cancelamento);

            var percentualReembolso = 0m;

            if (diasAntesDaEntrada >= TimeSpan.FromDays(2) && diasAntesDaEntrada <= TimeSpan.FromDays(7))
                percentualReembolso =0.5m; // 50% de desconto

            if(diasAntesDaEntrada < TimeSpan.FromDays(2))
                percentualReembolso = 0m; // Sem reembolso

            if (diasAntesDaEntrada > TimeSpan.FromDays(7))
               percentualReembolso = 1m;
  
            valorTotal = valorTotal * percentualReembolso;
            valorTotal = Math.Round(valorTotal, 2);

            return valorTotal;

        }

    }
}
