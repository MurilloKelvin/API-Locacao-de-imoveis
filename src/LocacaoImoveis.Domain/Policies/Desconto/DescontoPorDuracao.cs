using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Domain.Policies.Desconto
{
    public class DescontoPorDuracao : IPoliticaDesconto
    {
        private readonly int _duracaoMinima;
        private readonly decimal _percentualDesconto;

        public DescontoPorDuracao(int minimoNoites, decimal percentual)
        {

            if (minimoNoites < 1)
                throw new ArgumentException("O número de noites deve ser maior que zero.", nameof(minimoNoites));
            if (percentual <= 0)
                throw new ArgumentException("O percentual de desconto deve ser maior que zero.", nameof(percentual));

            _duracaoMinima = minimoNoites;
            _percentualDesconto = percentual;
        }

        public decimal CalcularTotal(decimal valorTotal, int duracao)
        {
            if (duracao >= _duracaoMinima)
            {
                var total = valorTotal;

                var desconto = valorTotal * _percentualDesconto / 100m;
                total -= desconto;

                return Math.Round(total, 2);
            }
            
            return valorTotal;
        }
    }

}