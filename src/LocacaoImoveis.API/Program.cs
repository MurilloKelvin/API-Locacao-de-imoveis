using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using LocacaoImoveis.Infrastructure;
using LocacaoImoveis.Application.Interfaces;
using LocacaoImoveis.Infrastructure.Repositories;
using LocacaoImoveis.Application.Services;

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


// config do ef
builder.Services.AddDbContext<AppDbContext>((ServiceProvider, options) =>
{
    var connection = ServiceProvider
    .GetRequiredService<SqliteConnection>();

    options.UseSqlite(connection);
});

builder.Services.AddOpenApi();

var app = builder.Build();

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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
