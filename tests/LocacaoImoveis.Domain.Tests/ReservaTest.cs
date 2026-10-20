using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;

namespace LocacaoImoveis.Domain.Tests;

public class ReservaTests
{
    [Fact]
    public void DeveCriarReservaPendente()
    {
        // cria uma reserva pendente com dados válidos
        var clienteId = Guid.NewGuid();
        var imovelId = Guid.NewGuid();
        var agora = new DateTimeOffset(
            2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3));

        var entrada = new DateOnly(2026, 10, 20);
        var saida = new DateOnly(2026, 10, 23);

        // inicia a reserva
        var reserva = new Reserva(
            clienteId, imovelId, entrada, saida, 2, 750m, agora);


        // verifica se a reserva foi criada corretamente
        Assert.NotEqual(Guid.Empty, reserva.Id);
        Assert.Equal(clienteId, reserva.ClienteId);
        Assert.Equal(imovelId, reserva.ImovelId);
        Assert.Equal(entrada, reserva.DataEntrada);
        Assert.Equal(saida, reserva.DataSaida);
        Assert.Equal(2, reserva.QuantidadeHospedes);
        Assert.Equal(750m, reserva.ValorTotalPreco);
        Assert.Equal(ReservaStatus.Pendente, reserva.Status);
        Assert.Equal(agora, reserva.CriadaEm);
        Assert.Equal(3, reserva.CalcularQuantidadeNoites());
        Assert.Null(reserva.CanceladaEm);
        Assert.Null(reserva.ValorReembolso);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    public void DeveAceitarDuracaoPermitida(int noites)
    {
        // faz a data de referência para a criação da reserva
        var agora = new DateTimeOffset(
            2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3));

        // cria uma data de entrada e saída com duração válida
        var entrada = new DateOnly(2026, 10, 20);
        var saida = entrada.AddDays(noites);

        // faz a reserva com a duração válida
        var reserva = new Reserva(
            Guid.NewGuid(), Guid.NewGuid(),
            entrada, saida, 2, 750m, agora);

        // verifica se a quantidade de noites calculada é igual à duração fornecida
        Assert.Equal(noites, reserva.CalcularQuantidadeNoites());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(31)]
    public void DeveRejeitarDuracaoInvalida(int noites)
    {
        // faz a data de referência para a criação da reserva
        var agora = new DateTimeOffset(
            2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3));

        // cria uma data de entrada e saída com duração inválida
        var entrada = new DateOnly(2026, 10, 20);
        var saida = entrada.AddDays(noites);

        // verifica se a exceção é lançada para durações inválidas
        Assert.Throws<ArgumentException>(() => new Reserva(
            Guid.NewGuid(), Guid.NewGuid(),
            entrada, saida, 2, 750m, agora));
    }

    [Fact]
    public void DeveRejeitarEntradaNoPassado()
    {
        // cria a data de referência para a criação da reserva
        var agora = new DateTimeOffset(
            2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3));

        // cria uma data de entrada no passado em relação à data de referência
        var entrada = new DateOnly(2026, 10, 18);
        var saida = new DateOnly(2026, 10, 20);

        // busca o erro esperado ao tentar criar a reserva com entrada no passado
        var erro = Assert.Throws<ArgumentException>(() => new Reserva(
            Guid.NewGuid(), Guid.NewGuid(),
            entrada, saida, 2, 500m, agora));

        Assert.Equal("dataEntrada", erro.ParamName);
    }

    [Fact]
    public void DevePermitirEntradaHoje()
    {
        // faz a data de referência para a criação da reserva
        var agora = new DateTimeOffset(
            2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3));

        var entrada = new DateOnly(2026, 10, 19);
        var saida = new DateOnly(2026, 10, 20);

        // cria a reserva com entrada no mesmo dia da criação
        var reserva = new Reserva(
            Guid.NewGuid(), Guid.NewGuid(),
            entrada, saida, 2, 250m, agora);

        // verifica se a reserva foi criada corretamente
        Assert.Equal(entrada, reserva.DataEntrada);
        Assert.Equal(1, reserva.CalcularQuantidadeNoites());
    }

    [Fact]
    public void DeveConfirmarReservaPendente()
    {
        // cria a data de referência para a criação da reserva
        var agora = new DateTimeOffset(
            2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3));

        var reserva = new Reserva(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 20),
            new DateOnly(2026, 10, 23),
            2,
            750m,
            agora);

        // tenta confirmar a reserva pendente
        reserva.Confirmar();

        // verifica se o status da reserva foi alterado para confirmada
        Assert.Equal(ReservaStatus.Confirmada, reserva.Status);
    }

    [Fact]
    public void DeveRejeitarConfirmacaoDeReservaJaConfirmada()
    {
        // data de referência para a criação da reserva
        var agora = new DateTimeOffset(
            2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3));

        var reserva = new Reserva(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 10, 20),
            new DateOnly(2026, 10, 23),
            2,
            750m,
            agora);

        reserva.Confirmar();

        // verifica se a tentativa de confirmar novamente a reserva confirmada lança uma exceção
        Assert.Throws<InvalidOperationException>(
            () => reserva.Confirmar());

        Assert.Equal(ReservaStatus.Confirmada, reserva.Status);
    }

    [Fact]
    public void DeveConcluirReservaConfirmada()
    {
        // cria a data de referência para a criação da reserva
        var agora = new DateTimeOffset(
            2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3));

        var reserva = new Reserva(
            Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2026, 10, 20),
            new DateOnly(2026, 10, 23),
            2, 750m, agora);

        reserva.Confirmar();

        // tenta concluir a reserva confirmada
        reserva.Concluir();

        // verifica se o status da reserva foi alterado para concluída
        Assert.Equal(ReservaStatus.Concluida, reserva.Status);
    }

    [Fact]
    public void DeveRejeitarConclusaoDeReservaPendente()
    {
        
        var agora = new DateTimeOffset(
            2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3));

        var reserva = new Reserva(
            Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2026, 10, 20),
            new DateOnly(2026, 10, 23),
            2, 750m, agora);

        // tenta concluir a reserva pendente
        Assert.Throws<InvalidOperationException>(
            () => reserva.Concluir());

        Assert.Equal(ReservaStatus.Pendente, reserva.Status);
    }

}