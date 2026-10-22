using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.DTOs.Imoveis
{
    public class ConsultarDisponibilidade
    {
        public DateOnly? Entrada { get; set; }
        public DateOnly? Saida { get; set; }
        public int QuantidadeHospedes { get; set; } = 1;
        public int Paginas { get; set; } = 1;
        public int ItensPorPagina { get; set; } = 10;
        public decimal? ValorDiarioMaximo { get; set; }
        public decimal? ValorDiarioMinimo { get; set; }
    }
}
