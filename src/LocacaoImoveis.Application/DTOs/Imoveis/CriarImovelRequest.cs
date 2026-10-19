using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.DTOs.Imoveis
{
    public class CriarImovelRequest
    {
        public string Endereco { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public int CapacidadeDeHospedes { get; set; }
        public decimal ValorDiaria { get; set; }
    }
}
