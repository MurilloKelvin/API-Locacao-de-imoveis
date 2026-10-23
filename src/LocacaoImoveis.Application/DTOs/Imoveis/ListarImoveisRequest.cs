using LocacaoImoveis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LocacaoImoveis.Application.DTOs.Imoveis
{
    public class ListarImoveisRequest
    {
        [Range(1,100)]
        public int Pagina { get; set; } = 1; // valor padrão para página
        [Range(1, 100)]

        public int TamanhoPagina { get; set; } = 10; // valor padrão para tamanho da página

        public ImovelStatus? Status { get; set; }
        public int? CapacidadeMaxima { get; set; }
        public int? CapacidadeMinima { get; set; }
        public decimal? ValorDiarioMinimo { get; set; }
        public decimal? ValorDiarioMaximo { get; set; }
    }
}
