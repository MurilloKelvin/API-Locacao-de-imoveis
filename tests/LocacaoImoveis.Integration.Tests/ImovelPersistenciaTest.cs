using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;
using LocacaoImoveis.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace LocacaoImoveis.Integration.Tests
{
    public class ImovelPersistenciaTest
    {
        [Fact]
        public async Task DevePersistirImovelNoBanco()
        {
            // Configurar o banco de dados em memória
            using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync(); 

            var options = new DbContextOptionsBuilder<AppDbContext>() 
                .UseSqlite(connection)
                .Options; 
            
            using var context = new AppDbContext(options); // faz 
            await context.Database.EnsureCreatedAsync();

            // cria um novo imovel
            var imovel = new Imovel(
                "Rua das Flores, 10",
                "Casa com piscina",
                6,
                250.50m
            );

            context.Imoveis.Add(imovel);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            // Recupera o imovel do banco de dados
            var imovelSalvo = await context.Imoveis.FindAsync(imovel.Id);

            // Assert
            Assert.NotNull(imovelSalvo);
            Assert.Equal(imovel.Id, imovelSalvo.Id);
            Assert.Equal("Rua das Flores, 10", imovelSalvo.Endereco);
            Assert.Equal("Casa com piscina", imovelSalvo.Titulo);
            Assert.Equal(6, imovelSalvo.CapacidadeDeHospedes);
            Assert.Equal(250.50m, imovelSalvo.ValorDiaria);
            Assert.Equal(ImovelStatus.Disponivel, imovelSalvo.Status);

        }
    }
}
