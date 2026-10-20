using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;
using LocacaoImoveis.Infrastructure;
using LocacaoImoveis.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LocacaoImoveis.Integration.Tests;

public class ReservaPersistenciaTest
{
    [Fact]
    public async Task DeveContarSomenteReservasAtivasDoImovel()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var cliente = new Cliente(
            "Cliente Teste",
            "11999999999",
            "cliente.teste@example.com",
            "52998224725");
        var imovel = new Imovel("Rua das Flores, 10", "Casa teste", 4, 250m);
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);

        var agora = DateTimeOffset.UtcNow;
        var reservaPendente = CriarReserva(cliente.Id, imovel.Id, agora, 1);
        var reservaConfirmada = CriarReserva(cliente.Id, imovel.Id, agora, 3);
        reservaConfirmada.Confirmar();
        var reservaConcluida = CriarReserva(cliente.Id, imovel.Id, agora, 5);
        reservaConcluida.Confirmar();
        reservaConcluida.Concluir();
        var reservaCancelada = CriarReserva(cliente.Id, imovel.Id, agora, 7);
        reservaCancelada.Cancelar(agora, 0m);

        context.Reservas.AddRange(
            reservaPendente,
            reservaConfirmada,
            reservaConcluida,
            reservaCancelada);
        await context.SaveChangesAsync();

        var repository = new ReservaRepository(context);

        var total = await repository.ContarReservasAtivas(imovel.Id);

        Assert.Equal(2, total);
    }

    private static Reserva CriarReserva(
        Guid clienteId,
        Guid imovelId,
        DateTimeOffset agora,
        int diasAteEntrada)
    {
        var entrada = DateOnly.FromDateTime(agora.DateTime).AddDays(diasAteEntrada);
        var saida = entrada.AddDays(2);

        return new Reserva(
            clienteId,
            imovelId,
            entrada,
            saida,
            2,
            500m,
            agora);
    }
}
