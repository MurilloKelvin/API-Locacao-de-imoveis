using LocacaoImoveis.API.Middleware;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Application.Services;
using LocacaoImoveis.Domain.Policies.Reembolso;
using LocacaoImoveis.Infrastructure;
using LocacaoImoveis.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using LocacaoImoveis.Domain.Policies.Desconto;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


// mantem a conexão com o banco de dados em memória aberta durante toda a execução da aplicação
builder.Services.AddSingleton<SqliteConnection>(_ =>
{
 var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open(); 
    return connection;
});

// registra o repositório e o serviço de cliente para injeção de dependência
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ClienteService>();

// dependencia do imovel
builder.Services.AddScoped<IImovelRepository, ImovelRepository>();
builder.Services.AddScoped<ImovelService>();

builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<ReservaService>();

builder.Services.AddScoped<PoliticaDeReembolso>();

//configuraçao do desconto 7 dias ou mais, 10% de desconto (VALOR PADRAO caso nao tenha no appsettings)
var minimoNoites = builder.Configuration.GetValue<int>("Desconto:MinimoNoites", 7);
var percentual = builder.Configuration.GetValue<decimal>("Desconto:Percentual", 10m);

// dependencia do desconto registra a politica na app toda
builder.Services.AddSingleton<IPoliticaDesconto>
    (new DescontoPorDuracao(minimoNoites, percentual));


// config do ef
builder.Services.AddDbContext<AppDbContext>((ServiceProvider, options) =>
{
    var connection = ServiceProvider
    .GetRequiredService<SqliteConnection>();

    options.UseSqlite(connection);
});


// adiciona health checks e openapi para documentação da API
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

// configura o middleware de tratamento de exceções
app.UseMiddleware<ExceptionHandlingMiddleware>();

// garante que o banco exista e que as tabelas sejam criadas
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API de Locação de Imóveis V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// health check endopoint :5088
app.MapHealthChecks("/health");

// garante que o banco de dados seja criado e que os dados iniciais sejam inseridos (seed)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    // pega a politica de desconto pra usar no seed
    var politicaDesconto = scope.ServiceProvider
        .GetRequiredService<IPoliticaDesconto>();

    // chama o método de seed para popular o banco de dados com dados iniciais
    await DbInitializer.SeedAsync(context, politicaDesconto);
}

app.Run();


// e usado pelo apiFactory para fazer os testes htpp usando o mesmo Program da API real
public partial class Program
{
}
