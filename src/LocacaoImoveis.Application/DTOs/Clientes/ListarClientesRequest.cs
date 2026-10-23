using System.ComponentModel.DataAnnotations;
using LocacaoImoveis.Domain.Enums;

namespace LocacaoImoveis.Application.DTOs.Clientes;

public class ListarClientesRequest
{
    [Range(1, int.MaxValue)]
    public int Pagina { get; set; } = 1;

    [Range(1, 100)]
    public int TamanhoPagina { get; set; } = 10;

    [EnumDataType(typeof(ClienteStatus))]
    public ClienteStatus? Status { get; set; }

    public string? Nome { get; set; }
}
