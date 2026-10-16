using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

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
            Assert.Equal("52998224725", clienteDoBanco.Cpf.Valor);



        }
                
    }
}
