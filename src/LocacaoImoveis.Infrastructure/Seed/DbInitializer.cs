using LocacaoImoveis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocacaoImoveis.Infrastructure;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
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

        await context.SaveChangesAsync();
    }
}