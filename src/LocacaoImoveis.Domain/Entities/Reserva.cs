using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Enums;
using LocacaoImoveis.Domain.Entities;

namespace LocacaoImoveis.Domain.Entities
{
    public class Reserva
    {
        public Guid Id { get;private set; }
        public Guid ClienteId { get; private set; }
        public Guid ImovelId { get; private set; }
        public DateOnly DataEntrada { get; private set; }
        public DateOnly DataSaida { get; private set; }
        public int QuantidadeHospedes { get; private set; }
        public decimal ValorTotalPreco { get; private set; }
        public ReservaStatus Status { get; private set; }
        public DateTimeOffset CriadaEm { get; private set; }
        public DateTimeOffset? CanceladaEm { get; private set; }
        public decimal? ValorReembolso { get; private set; }

        private Reserva() { } // Construtor privado para EF Core

        // Construtor da classe 
        public Reserva(
            Guid clienteId, 
            Guid imovelId,
            DateOnly dataEntrada,
            DateOnly dataSaida,
            int quantidadeHospedes,
            decimal valorTotalPreco,
            DateTimeOffset agora
            )
        { 
            if(clienteId == Guid.Empty)
                throw new ArgumentException(
                    "ClienteId não pode ser vazio.", nameof(clienteId));

            if(imovelId == Guid.Empty)
                throw new ArgumentException(
                    "ImovelId não pode ser vazio.", nameof(imovelId));

            if(dataEntrada>= dataSaida)
                throw new ArgumentException(
                    "Data de entrada deve ser antes que a data de saída.", nameof(dataEntrada));

            if(dataEntrada < DateOnly.FromDateTime(agora.DateTime))
                throw new ArgumentException(
                    "Data de entrada não pode ser no passado.", nameof(dataEntrada));

            if(dataSaida < DateOnly.FromDateTime(agora.DateTime))
                throw new ArgumentException(
                    "Data de saída não pode ser no passado.", nameof(dataSaida));

            // Valida se a reserva não ultrapassa 30 dias
            if ((dataSaida.DayNumber - dataEntrada.DayNumber) > 30)
                throw new ArgumentException(
                    "Reserva não pode ter mais de 30 dias.", nameof(dataSaida));

            if (quantidadeHospedes <= 0)
                throw new ArgumentException(
                    "Quantidade de hóspedes deve ser maior que zero.", nameof(quantidadeHospedes));

            if(valorTotalPreco <= 0)
                throw new ArgumentException(
                    "Valor total da reserva deve ser maior que zero.", nameof(valorTotalPreco));
        

            Id = Guid.NewGuid();
            ClienteId = clienteId;
            ImovelId = imovelId;
            DataEntrada = dataEntrada;
            DataSaida = dataSaida;
            QuantidadeHospedes = quantidadeHospedes;
            ValorTotalPreco = valorTotalPreco;
            Status = ReservaStatus.Pendente;
            CriadaEm = agora;


        }

        public int CalcularQuantidadeNoites() => (DataSaida.DayNumber - DataEntrada.DayNumber);
        
         public void Confirmar()
        {
            if(Status != ReservaStatus.Pendente)
                throw new InvalidOperationException(
                    "Somente reservas pendentes podem ser confirmadas.");

            Status = ReservaStatus.Confirmada;
        }

        public void Concluir()
        {
            if(Status != ReservaStatus.Confirmada)
                throw new InvalidOperationException(
                    "Somente reservas confirmadas podem ser concluídas.");

            Status = ReservaStatus.Concluida;
        }

        public void Cancelar(DateTimeOffset agora, decimal valorReembolso)
        {
            if (Status != ReservaStatus.Pendente &&
                Status != ReservaStatus.Confirmada)
            {
                throw new InvalidOperationException(
                    "Somente reservas pendentes ou confirmadas podem ser canceladas.");
            }

            if (agora < CriadaEm)
                throw new ArgumentException(
                    "O cancelamento não pode ocorrer antes da criação.",
                    nameof(agora));

            if (valorReembolso < 0 || valorReembolso > ValorTotalPreco)
                throw new ArgumentException(
                    "O reembolso deve estar entre zero e o valor total da reserva.",
                    nameof(valorReembolso));

            Status = ReservaStatus.Cancelada;
            CanceladaEm = agora;
            ValorReembolso = valorReembolso;
        }




    }
}
