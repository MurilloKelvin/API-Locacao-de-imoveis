using System.ComponentModel.DataAnnotations;
using LocacaoImoveis.Domain.Enums;

namespace LocacaoImoveis.Application.DTOs.Reservas;

public class ListarReservasRequest
{
    [Range(1, int.MaxValue)]
    public int Pagina { get; set; } = 1;

    [Range(1, 100)]
    public int TamanhoPagina { get; set; } = 10;

    [EnumDataType(typeof(ReservaStatus))]
    public ReservaStatus? Status { get; set; }

    public Guid? ClienteId { get; set; }
    public Guid? ImovelId { get; set; }
}
