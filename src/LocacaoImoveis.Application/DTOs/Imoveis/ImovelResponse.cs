using LocacaoImoveis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.DTOs.Imoveis
{
    public class ImovelResponse
    {
        public Guid Id { get; set; }
        public string Endereco { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public int CapacidadeHospedes { get; set; }
        public decimal ValorDiaria { get; set; }
        public ImovelStatus Status { get; set; }
    }
}
