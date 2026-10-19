using LocacaoImoveis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.DTOs.Imoveis
{
    public class ListarImoveisRequest
    {
        public int Pagina { get; set; } = 1; // valor padrão para página
        public int TamanhoPagina { get; set; } = 10; // valor padrão para tamanho da página

        public ImovelStatus? Status { get; set; }
        public int? CapacidadeMaxima { get; set; }
        public int? CapacidadeMinima { get; set; }
        public decimal? ValorDiarioMinimo { get; set; }
        public decimal? ValorDiarioMaximo { get; set; }
    }
}
