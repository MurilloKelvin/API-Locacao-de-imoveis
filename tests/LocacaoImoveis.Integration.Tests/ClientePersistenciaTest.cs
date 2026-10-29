using LocacaoImoveis.Application.Exceptions;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Infrastructure;
using LocacaoImoveis.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LocacaoImoveis.Integration.Tests
{
    public class ClientePersistenciaTest
    {
        [Fact]
        public void DevePersistirClienteNoBanco()
        {
            // Configurar o banco de dados em memória
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            // Configurar o DbContext para usar o banco de dados em memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            // Criar o banco de dados e aplicar as migrações
            using var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            // criar um cliente de teste 
            var cliente = new Cliente(
                "teste da silva",
               "11999999999",
               "testedasilva@hotmail.com",
               "52998224725"
             );

            context.Clientes.Add(cliente); // Adiciona o cliente ao contexto
            context.SaveChanges(); // Salva as alterações no banco de dados

            context.ChangeTracker.Clear(); // esquece o objeto pra simular uma nova consulta ao banco

            var clienteDoBanco = context.Clientes.Find(cliente.Id); //busca o cliente no banco de dados pelo Id

            //compara os dados 
            Assert.NotNull(clienteDoBanco);
            Assert.Equal("teste da silva", clienteDoBanco.Nome);
            Assert.Equal("11999999999", clienteDoBanco.Telefone);
            Assert.Equal("testedasilva@hotmail.com", clienteDoBanco.Email.Valor); // 
            Assert.Equal("52998224725", clienteDoBanco.DocumentoCliente.Valor);

        }

        [Fact]
        public void RejeitarDuplicidadeDeCpf()
        {
            // Configurar o banco de dados em memória
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            // Configurar o DbContext para usar o banco de dados em memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            // Criar o banco de dados e aplicar as migrações
            using var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            
            // criar um cliente de teste 
            var cliente1 = new Cliente(
                "teste da silva",
               "11999999999",
               "testedasilva@hotmail.com",
               "52998224725"
             );

            context.Clientes.Add(cliente1); // Adiciona o cliente ao contexto
            context.SaveChanges();

            var cliente2 = new Cliente(
               "teste da silva",
              "11999999999",
              "testedasilva2@hotmail.com",
              "52998224725"
            );

            context.Clientes.Add(cliente2); // Adiciona o cliente ao contexto

            var erro = Assert.Throws<DbUpdateException>(()
                => context.SaveChanges()); // Salva as alterações no banco de dados

            var erroEsperado = Assert.IsType<SqliteException>(erro.InnerException);
            
            Assert.Equal(2067, erroEsperado.SqliteExtendedErrorCode); // 2067 é o código de erro de violação de chave única no SQLite
        }

        [Fact]
        public async Task DeveDarErroDeCpfDuplicado()
        {
            // Arrange
            using var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            var repository = new ClienteRepository(context);

            var cliente1 = new Cliente(
                "Ana", "31999999999", "ana@email.com", "52998224725");

            await repository.AdicionarClienteAsync(cliente1);

            var cliente2 = new Cliente(
                "Bruno", "31988888888", "bruno@email.com", "52998224725");

            
            await Assert.ThrowsAsync<CpfJaCadastradoException>(
                () => repository.AdicionarClienteAsync(cliente2));
        }

    }
}
