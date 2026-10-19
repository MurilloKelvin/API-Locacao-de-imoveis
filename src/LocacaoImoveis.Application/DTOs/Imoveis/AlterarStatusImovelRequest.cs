using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Enums;
using LocacaoImoveis.Domain.Entities;

namespace LocacaoImoveis.Application.DTOs.Imoveis
{
    public class AlterarStatusImovelRequest
    {
        public ImovelStatus? Status { get; set; }
    }
}
