using LocacaoImoveis.Application.Services;
using LocacaoImoveis.Application.DTOs.Reservas;
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
        // Cria uma conexão SQLite em memória para isolar o teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        // Configura o EF Core para utilizar a conexão SQLite do teste.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Cria o contexto e as tabelas do banco em memória.
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria o cliente e o imóvel usados nas reservas.
        var cliente = new Cliente(
            "Cliente Teste",
            "11999999999",
            "cliente.teste@example.com",
            "52998224725");
        var imovel = new Imovel("Rua das Flores, 10", "Casa teste", 4, 250m);
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);

        // Cria reservas com os quatro status possíveis.
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

        // Cria o repositório que será testado.
        var repository = new ReservaRepository(context);

        // Conta somente as reservas pendentes e confirmadas.
        var total = await repository.ContarReservasAtivas(imovel.Id);

        // Duas das quatro reservas devem ser consideradas ativas.
        Assert.Equal(2, total);
    }

    [Fact]
    public async Task DeveBloquearQuartaReservaAtivaDoMesmoCliente()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var cliente = CriarCliente();
        var imoveis = Enumerable.Range(1, 4)
            .Select(indice => CriarImovel($"Imóvel limite {indice}"))
            .ToList();

        context.Clientes.Add(cliente);
        context.Imoveis.AddRange(imoveis);
        await context.SaveChangesAsync();

        var service = new ReservaService(
            new ReservaRepository(context),
            new ImovelRepository(context),
            new ClienteRepository(context));

        var dataBase = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        for (var indice = 0; indice < 3; indice++)
        {
            var entrada = dataBase.AddDays(indice * 3);

            await service.CriarReservaAsync(new CriarReservaRequest
            {
                ClienteId = cliente.Id,
                ImovelId = imoveis[indice].Id,
                DataEntrada = entrada,
                DataSaida = entrada.AddDays(2),
                QuantidadeHospedes = 2
            });
        }

        var entradaQuartaReserva = dataBase.AddDays(9);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CriarReservaAsync(new CriarReservaRequest
            {
                ClienteId = cliente.Id,
                ImovelId = imoveis[3].Id,
                DataEntrada = entradaQuartaReserva,
                DataSaida = entradaQuartaReserva.AddDays(2),
                QuantidadeHospedes = 2
            }));

        var total = await new ReservaRepository(context)
            .ContarReservasAtivasPorCliente(cliente.Id);

        Assert.Equal(3, total);
    }

    [Fact]
    public async Task DeveConfirmarReservaPendente()
    {
        // Cria uma conexão SQLite em memória para isolar o teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        // Configura o EF Core para utilizar a conexão SQLite do teste.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Cria o contexto e as tabelas do banco em memória.
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria e persiste o cliente e o imóvel da reserva.
        var cliente = CriarCliente();
        var imovel = CriarImovel("Imóvel para confirmação");
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);
        await context.SaveChangesAsync();

        // Cria o serviço utilizando os repositórios reais com o banco em memória.
        var service = new ReservaService(
            new ReservaRepository(context),
            new ImovelRepository(context),
            new ClienteRepository(context));

        // Define um período futuro válido para a reserva.
        var agora = DateTimeOffset.UtcNow;
        var entrada = DateOnly.FromDateTime(agora.DateTime).AddDays(1);

        // Cria a reserva, que deve iniciar com status pendente.
        var reservaId = await service.CriarReservaAsync(new CriarReservaRequest
        {
            ClienteId = cliente.Id,
            ImovelId = imovel.Id,
            DataEntrada = entrada,
            DataSaida = entrada.AddDays(2),
            QuantidadeHospedes = 2
        });

        // Confirma a reserva por meio do serviço.
        var resultado = await service.ConfirmarReservaAsync(reservaId);

        // Busca a reserva novamente para verificar o status persistido.
        var reserva = await new ReservaRepository(context)
            .ObterIdAsync(reservaId);

        // A confirmação deve ser concluída com sucesso.
        Assert.True(resultado);
        Assert.NotNull(reserva);

        // O status da reserva deve ser alterado para confirmado.
        Assert.Equal(ReservaStatus.Confirmada, reserva.Status);
    }

    [Fact]
    public async Task DeveConcluirReservaConfirmada()
    {
        // Cria uma conexão SQLite em memória para isolar o teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        // Configura o EF Core para utilizar a conexão SQLite do teste.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Cria o contexto e as tabelas do banco em memória.
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria e persiste o cliente e o imóvel da reserva.
        var cliente = CriarCliente();
        var imovel = CriarImovel("Imóvel para conclusão");
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);
        await context.SaveChangesAsync();

        // Cria o serviço utilizando os repositórios reais com o banco em memória.
        var service = new ReservaService(
            new ReservaRepository(context),
            new ImovelRepository(context),
            new ClienteRepository(context));

        // Define um período futuro válido para a reserva.
        var agora = DateTimeOffset.UtcNow;
        var entrada = DateOnly.FromDateTime(agora.DateTime).AddDays(1);

        // Cria uma reserva pendente.
        var reservaId = await service.CriarReservaAsync(new CriarReservaRequest
        {
            ClienteId = cliente.Id,
            ImovelId = imovel.Id,
            DataEntrada = entrada,
            DataSaida = entrada.AddDays(2),
            QuantidadeHospedes = 2
        });

        // Confirma a reserva antes de tentar concluí-la.
        await service.ConfirmarReservaAsync(reservaId);

        // Conclui a reserva por meio do serviço.
        var resultado = await service.ConcluirReservaAsync(reservaId);

        // Busca a reserva novamente para verificar o status persistido.
        var reserva = await new ReservaRepository(context)
            .ObterIdAsync(reservaId);

        // A conclusão deve ser executada com sucesso.
        Assert.True(resultado);
        Assert.NotNull(reserva);

        // O status da reserva deve ser alterado para concluído.
        Assert.Equal(ReservaStatus.Concluida, reserva.Status);
    }

    [Fact]
    public async Task DeveCancelarReservaComReembolsoValido()
    {
        // Cria uma conexão SQLite em memória para isolar o teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        // Configura o EF Core para utilizar a conexão SQLite do teste.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Cria o contexto e as tabelas do banco em memória.
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria e persiste o cliente e o imóvel da reserva.
        var cliente = CriarCliente();
        var imovel = CriarImovel("Imóvel para cancelamento");
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);
        await context.SaveChangesAsync();

        // Cria o serviço utilizando os repositórios reais com o banco em memória.
        var service = new ReservaService(
            new ReservaRepository(context),
            new ImovelRepository(context),
            new ClienteRepository(context));

        // Define um período futuro válido para a reserva.
        var agora = DateTimeOffset.UtcNow;
        var entrada = DateOnly.FromDateTime(agora.DateTime).AddDays(1);

        // Cria uma reserva pendente para ser cancelada.
        var reservaId = await service.CriarReservaAsync(new CriarReservaRequest
        {
            ClienteId = cliente.Id,
            ImovelId = imovel.Id,
            DataEntrada = entrada,
            DataSaida = entrada.AddDays(2),
            QuantidadeHospedes = 2
        });

        // Cancela a reserva informando um valor de reembolso válido.
        var resultado = await service.CancelarReservaAsync(reservaId, 100m);

        // Busca a reserva novamente para verificar os dados persistidos.
        var reserva = await new ReservaRepository(context)
            .ObterIdAsync(reservaId);

        // O cancelamento deve ser executado com sucesso.
        Assert.True(resultado);
        Assert.NotNull(reserva);

        // O status, o valor do reembolso e a data de cancelamento devem ser registrados.
        Assert.Equal(ReservaStatus.Cancelada, reserva.Status);
        Assert.Equal(100m, reserva.ValorReembolso);
        Assert.NotNull(reserva.CanceladaEm);
    }

    [Fact]
    public async Task DeveDetectarConflitoComReservaPendente()
    {
        // Cria o banco de dados em memória utilizado pelo teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria o cliente e o imóvel relacionados à reserva.
        var cliente = CriarCliente();
        var imovel = CriarImovel("Imóvel com reserva pendente");
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);

        var agora = DateTimeOffset.UtcNow;
        var reserva = CriarReserva(cliente.Id, imovel.Id, agora, 2);
        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();

        var repository = new ReservaRepository(context);
        var entradaSolicitada = DateOnly.FromDateTime(agora.DateTime).AddDays(3);
        var saidaSolicitada = entradaSolicitada.AddDays(2);

        // A reserva pendente ocupa parte do período solicitado.
        var existeConflito = await repository.ExisteConflitos(
            imovel.Id,
            entradaSolicitada,
            saidaSolicitada);

        // Reservas pendentes devem bloquear períodos sobrepostos.
        Assert.True(existeConflito);
    }

    [Fact]
    public async Task DeveDetectarConflitoComReservaConfirmada()
    {
        // Cria o banco de dados em memória utilizado pelo teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria o cliente e o imóvel relacionados à reserva.
        var cliente = CriarCliente();
        var imovel = CriarImovel("Imóvel com reserva confirmada");
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);

        var agora = DateTimeOffset.UtcNow;
        var reserva = CriarReserva(cliente.Id, imovel.Id, agora, 2);
        reserva.Confirmar();
        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();

        var repository = new ReservaRepository(context);
        var entradaSolicitada = DateOnly.FromDateTime(agora.DateTime).AddDays(3);
        var saidaSolicitada = entradaSolicitada.AddDays(2);

        // A reserva confirmada ocupa parte do período solicitado.
        var existeConflito = await repository.ExisteConflitos(
            imovel.Id,
            entradaSolicitada,
            saidaSolicitada);

        // Reservas confirmadas devem bloquear períodos sobrepostos.
        Assert.True(existeConflito);
    }

    [Fact]
    public async Task NaoDeveDetectarConflitoComReservaCancelada()
    {
        // Cria o banco de dados em memória utilizado pelo teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria o cliente e o imóvel relacionados à reserva.
        var cliente = CriarCliente();
        var imovel = CriarImovel("Imóvel com reserva cancelada");
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);

        var agora = DateTimeOffset.UtcNow;
        var reserva = CriarReserva(cliente.Id, imovel.Id, agora, 2);
        reserva.Cancelar(agora, 0m);
        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();

        var repository = new ReservaRepository(context);
        var entradaSolicitada = DateOnly.FromDateTime(agora.DateTime).AddDays(3);
        var saidaSolicitada = entradaSolicitada.AddDays(2);

        // Consulta um período que coincide com a reserva cancelada.
        var existeConflito = await repository.ExisteConflitos(
            imovel.Id,
            entradaSolicitada,
            saidaSolicitada);

        // Reservas canceladas não devem bloquear novas reservas.
        Assert.False(existeConflito);
    }

    [Fact]
    public async Task NaoDeveDetectarConflitoComReservaDeOutroImovel()
    {
        // Cria o banco de dados em memória utilizado pelo teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria um cliente e dois imóveis diferentes.
        var cliente = CriarCliente();
        var imovelComReserva = CriarImovel("Imóvel com reserva");
        var imovelConsultado = CriarImovel("Imóvel consultado");
        context.Clientes.Add(cliente);
        context.Imoveis.AddRange(imovelComReserva, imovelConsultado);

        var agora = DateTimeOffset.UtcNow;
        var reserva = CriarReserva(cliente.Id, imovelComReserva.Id, agora, 2);
        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();

        var repository = new ReservaRepository(context);
        var entradaSolicitada = DateOnly.FromDateTime(agora.DateTime).AddDays(3);
        var saidaSolicitada = entradaSolicitada.AddDays(2);

        // Consulta o período no imóvel que não possui a reserva.
        var existeConflito = await repository.ExisteConflitos(
            imovelConsultado.Id,
            entradaSolicitada,
            saidaSolicitada);

        // Uma reserva de outro imóvel não pode gerar conflito.
        Assert.False(existeConflito);
    }

    [Fact]
    public async Task NaoDeveDetectarConflitoComReservaForaDoPeriodo()
    {
        // Cria o banco de dados em memória utilizado pelo teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria o cliente e o imóvel relacionados à reserva.
        var cliente = CriarCliente();
        var imovel = CriarImovel("Imóvel sem conflito de período");
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);

        var agora = DateTimeOffset.UtcNow;
        var reserva = CriarReserva(cliente.Id, imovel.Id, agora, 1);
        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();

        var repository = new ReservaRepository(context);
        var entradaSolicitada = DateOnly.FromDateTime(agora.DateTime).AddDays(5);
        var saidaSolicitada = entradaSolicitada.AddDays(2);

        // Consulta um período posterior ao término da reserva existente.
        var existeConflito = await repository.ExisteConflitos(
            imovel.Id,
            entradaSolicitada,
            saidaSolicitada);

        // Reservas que não se sobrepõem ao período solicitado não geram conflito.
        Assert.False(existeConflito);
    }

    [Fact]
    public async Task DeveListarReservasComQuantidadeDeNoitesCorreta()
    {
        // Cria uma conexão SQLite em memória para isolar o teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        // Configura o EF Core para utilizar a conexão SQLite do teste.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Cria o contexto e as tabelas do banco em memória.
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria o cliente e o imóvel usados nas reservas.
        var cliente = CriarCliente();
        var imovel = CriarImovel("Imóvel para listagem");
        context.Clientes.Add(cliente);
        context.Imoveis.Add(imovel);

        // Cria reservas com quantidades diferentes de noites.
        var agora = DateTimeOffset.UtcNow;
        var reservaDeDuasNoites = CriarReserva(cliente.Id, imovel.Id, agora, 1, 2);
        var reservaDeCincoNoites = CriarReserva(cliente.Id, imovel.Id, agora, 5, 5);
        context.Reservas.AddRange(reservaDeDuasNoites, reservaDeCincoNoites);
        await context.SaveChangesAsync();

        // Cria o serviço para testar a conversão para ReservaResponse.
        var service = new ReservaService(
            new ReservaRepository(context),
            new ImovelRepository(context),
            new ClienteRepository(context));

        // Solicita a listagem de reservas.
        var reservas = await service.ObterTodasReservasAsync();

        // Confirma que as duas reservas foram retornadas.
        Assert.Equal(2, reservas.Count);

        // Confirma que cada reserva recebeu sua própria quantidade de noites.
        Assert.Contains(reservas, reserva => reserva.QuantidadeDeNoites == 2);
        Assert.Contains(reservas, reserva => reserva.QuantidadeDeNoites == 5);
    }

    [Fact]
    public async Task DeveRetornarListaVaziaQuandoNaoExistiremReservas()
    {
        // Cria uma conexão SQLite em memória para isolar o teste.
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        // Configura o EF Core para utilizar a conexão SQLite do teste.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Cria o contexto e as tabelas do banco em memória.
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Cria o serviço sem inserir reservas no banco.
        var service = new ReservaService(
            new ReservaRepository(context),
            new ImovelRepository(context),
            new ClienteRepository(context));

        // Solicita a listagem de reservas.
        var reservas = await service.ObterTodasReservasAsync();

        // Uma consulta sem registros deve retornar uma lista vazia.
        Assert.Empty(reservas);
    }

    private static Reserva CriarReserva(
        Guid clienteId,
        Guid imovelId,
        DateTimeOffset agora,
        int diasAteEntrada,
        int quantidadeDeNoites = 2)
    {
        var entrada = DateOnly.FromDateTime(agora.DateTime).AddDays(diasAteEntrada);
        var saida = entrada.AddDays(quantidadeDeNoites);

        return new Reserva(
            clienteId,
            imovelId,
            entrada,
            saida,
            2,
            500m,
            agora);
    }

    private static Cliente CriarCliente()
    {
        // Cria um cliente válido para atender à chave estrangeira da reserva.
        // Cada teste usa um banco isolado, portanto o mesmo CPF pode ser reutilizado.
        return new Cliente(
            "Cliente Teste",
            "11999999999",
            "cliente.teste@example.com",
            "52998224725");
    }

    private static Imovel CriarImovel(string titulo)
    {
        // Cria um imóvel válido para ser utilizado nos cenários de conflito.
        return new Imovel(
            "Rua das Flores, 10",
            titulo,
            4,
            250m);
    }

}
