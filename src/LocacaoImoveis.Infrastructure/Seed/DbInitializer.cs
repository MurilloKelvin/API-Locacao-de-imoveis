using LocacaoImoveis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using LocacaoImoveis.Domain.Policies.Desconto;

namespace LocacaoImoveis.Infrastructure;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, IPoliticaDesconto politicaDesconto)
    {
        if (await context.Clientes.AnyAsync())
            return;

        // clientes
        var cliente = new Cliente(
            "Pedro junior",
            "11999999999",
            "pedro.junior@teste.com",
            "04462462041");

        var cliente2 = new Cliente(
            "Roberto Carlos",
            "11999999999",
            "roberto.carlos@teste.com",
            "52998224725");

        var cliente3 = new Cliente(
            "Lucas silva",
            "11999999999",
            "lucas.silva@teste.com",
            "29875676004");

        //imoveis
        var imovel = new Imovel(
            "Rua da amora, 100",
            "Casa da amora",
            4,
           175m);

        var imovel2 = new Imovel(
            "Rua 22, 1890",
            "Cobertura esquina",
            4,
            500m);

        var imovel3 = new Imovel(
           "Rua pampulha, 20",
           "Casa Pampulha",
           6,
           100m);


        //add os clientes
        context.Clientes.Add(cliente);
        context.Clientes.Add(cliente2);

        // add os imoveis ao context(db
        context.Imoveis.Add(imovel);
        context.Imoveis.Add(imovel2);
        context.Imoveis.Add(imovel3);


        // reservas
        var agora = DateTimeOffset.UtcNow
            .ToOffset(TimeSpan.FromHours(-3));

        var entrada = DateOnly.FromDateTime(agora.DateTime).AddDays(10);
        var saida = entrada.AddDays(3);

        var quantidadeNoites = saida.DayNumber - entrada.DayNumber;
        var valorTotal = quantidadeNoites * imovel.ValorDiaria;

        var reserva = new Reserva(
            cliente.Id,
            imovel.Id,
            entrada,
            saida,
            2,
            valorTotal,
            agora
        );


        var entrada2 = DateOnly.FromDateTime(agora.DateTime).AddDays(2);
        var saida2 = entrada.AddDays(3);
        var quantidadeNoites2 = saida2.DayNumber - entrada2.DayNumber;

        var subTotal2 = imovel2.ValorDiaria * quantidadeNoites2;
        var valorTotal2 = politicaDesconto.CalcularTotal(subTotal2, quantidadeNoites2);

        var reserva2 = new Reserva(
            cliente2.Id,
            imovel2.Id,
            entrada2,
            saida2,
            2,
            valorTotal2,
            agora
        );

        context.Reservas.Add(reserva);
        context.Reservas.Add(reserva2);



        await context.SaveChangesAsync();
    }
}