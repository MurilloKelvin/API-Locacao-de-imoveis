using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;
using LocacaoImoveis.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LocacaoImoveis.API.Jobs;

// background roda enquanto a api ta ligaada
public class InativarClientesJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InativarClientesJob> _logger;

    public InativarClientesJob(
        IServiceScopeFactory scopeFactory,
        ILogger<InativarClientesJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // roda quando a api inicia dps espera 24h (1dia)
    // stoppingToken é usado para parar o job caso a api seja desligada
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecutarAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromDays(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecutarAsync(stoppingToken);
        }
    }


    private async Task ExecutarAsync(CancellationToken cancellationToken)
    {

        //carregaa o escopo de serviço para acessar o banco de dados
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // busca so os cliente ativo
        var clientes = await db.Clientes
            .Where(c => c.Status == ClienteStatus.Ativo)
            .ToListAsync(cancellationToken);

        var reservas = await db.Reservas
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // data de 3 meses tras
        var limite = DateOnly.FromDateTime(
            DateTimeOffset.UtcNow.AddMonths(-3).UtcDateTime);

        var inativados = 0;

        // roda todos cliente ativos e os que n tiver reserva nos 3 ultimos meses inativa
        foreach (var cliente in clientes)
        {
            var reservasDoCliente = reservas
                .Where(r => r.ClienteId == cliente.Id)
                .ToList();

            if (DeveInativar(cliente, reservasDoCliente, limite))
            {
                cliente.InativarCliente();
                inativados++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Verificação de clientes concluída. Inativados: {Quantidade}",
            inativados);
    }

    internal static bool DeveInativar(
        Cliente cliente,
        List<Reserva> reservas,
        DateOnly limite)
    {

        // se tiver reserva pendente ou confirmada n pode inativar
        if (reservas.Any(r =>
            r.Status is ReservaStatus.Pendente or ReservaStatus.Confirmada))
        {
            return false;
        }

        // veerifica a ultima atividade do cliente
        var ultimaAtividade =
            DateOnly.FromDateTime(cliente.CriadoEm.UtcDateTime);

        // se a ultima atividade for antes do limite, o cliente deve ser inativado
        foreach (var reserva in reservas)
        {
            var data = reserva.Status switch
            {
                ReservaStatus.Concluida => reserva.DataSaida,

                // se a reserva foi cancelada, a ultima atividade é a data de cancelamento ou a data de criação da reserva
                ReservaStatus.Cancelada => DateOnly.FromDateTime(
                    (reserva.CanceladaEm ?? reserva.CriadaEm).UtcDateTime),

                // se a reserva foi pendente ou confirmada, a ultima atividade é a data de criação da reserva
                _ => DateOnly.FromDateTime(reserva.CriadaEm.UtcDateTime)
            };
            // se a data da reserva for maior que a ultima atividade atualiza pra data da reserva
            if (data > ultimaAtividade)
                ultimaAtividade = data;
        }

        // se a ultima atividade for menor ou igual ao limite, o cliente deve ser inativado retorna true se for inativo
        return ultimaAtividade <= limite;
    }
}