using LocacaoImoveis.Domain.Enums;

namespace LocacaoImoveis.Application.DTOs.Reservas;

public class ReservaResponse
{
    public Guid Id { get; set; }

    public Guid ClienteId { get; set; }

    public Guid ImovelId { get; set; }

    public DateOnly DataEntrada { get; set; }
    public DateOnly DataSaida { get; set; }

    public int QuantidadeDeNoites { get; set; }

    public int QuantidadeHospedes { get; set; }

    public decimal ValorTotalPreco { get; set; }

    public ReservaStatus Status { get; set; }

    public DateTimeOffset CriadaEm { get; set; }

    //dados vazios caso a reserva não tenha sido cancelada
    public DateTimeOffset? CanceladaEm { get; set; }
    public decimal? ValorReembolso { get; set; }
}