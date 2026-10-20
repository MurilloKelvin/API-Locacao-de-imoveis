using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.DTOs.Reservas
{
    public class CriarReservaRequest
    {
        public Guid ClienteId { get; set; }
        public Guid ImovelId { get; set; }
        public DateOnly DataEntrada { get; set; }
        public DateOnly DataSaida { get; set; }
        public int QuantidadeHospedes { get; set; }
    }
}
