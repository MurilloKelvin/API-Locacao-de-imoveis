using LocacaoImoveis.Application.DTOs.Imoveis;
using LocacaoImoveis.Application.Services;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;
using LocacaoImoveis.Infrastructure;
using LocacaoImoveis.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LocacaoImoveis.Integration.Tests;

public class ImovelServiceTest
{
    [Fact]
    public async Task DeveImpedirAlterarImovelParaIndisponivelComReservaAtiva()
    {
        // Cria uma conexão SQLite em memória para isolar o teste.
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        // Configura o EF Core para utilizar a conexão SQLite do teste.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Cria o contexto e as tabelas do banco em memória.
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria os dados necessários para relacionar a reserva ao cliente e ao imóvel.
        var cliente = new Cliente(
            "Cliente Teste",
            "11999999999",
            "cliente.teste@example.com",
            "52998224725");

        var imovel = new Imovel(
            "Rua das Flores, 10",
            "Casa teste",
            4,
            250m);

        // Persiste o cliente e o imóvel antes de criar a reserva.
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);
        await context.SaveChangesAsync();

        // Define um período futuro válido para a reserva.
        var agora = DateTimeOffset.UtcNow;
        var entrada = DateOnly.FromDateTime(agora.DateTime).AddDays(1);
        var saida = entrada.AddDays(2);

        // Cria uma reserva pendente, que deve ser considerada ativa.
        var reserva = new Reserva(
            cliente.Id,
            imovel.Id,
            entrada,
            saida,
            2,
            500m,
            agora);

        // Persiste a reserva para que o contador possa encontrá-la.
        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();

        // Cria o serviço utilizando os repositórios reais com o banco em memória.
        var service = new ImovelService(
            new ImovelRepository(context),
            new ReservaRepository(context),
            NullLogger<ImovelService>.Instance);

        // Solicita a alteração do imóvel para indisponível.
        var request = new AlterarStatusImovelRequest
        {
            Status = ImovelStatus.Indisponivel
        };

        // A alteração deve ser rejeitada porque existe uma reserva ativa.
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AlterarStatusImovelAsync(imovel.Id, request));

        // Confirma que o status original do imóvel foi preservado.
        Assert.Equal(ImovelStatus.Disponivel, imovel.Status);
    }

    [Fact]
    public async Task DevePermitirAlterarImovelParaIndisponivelSemReservaAtiva()
    {
        // Cria uma conexão SQLite em memória para isolar o teste.
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        // Configura o EF Core para utilizar a conexão SQLite do teste.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Cria o contexto e as tabelas do banco em memória.
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria e persiste um imóvel sem reservas vinculadas.
        var imovel = new Imovel(
            "Rua das Flores, 10",
            "Casa teste",
            4,
            250m);

        context.Imoveis.Add(imovel);
        await context.SaveChangesAsync();

        // Cria o serviço utilizando os repositórios reais com o banco em memória.
        var service = new ImovelService(
            new ImovelRepository(context),
            new ReservaRepository(context),
            NullLogger<ImovelService>.Instance);

        // Solicita a alteração do imóvel para indisponível.
        var request = new AlterarStatusImovelRequest
        {
            Status = ImovelStatus.Indisponivel
        };

        // Executa a alteração; sem reservas ativas, ela deve ser permitida.
        await service.AlterarStatusImovelAsync(
            imovel.Id,
            request);

        // Confirma que o status foi atualizado.
        Assert.Equal(ImovelStatus.Indisponivel, imovel.Status);
    }
}
