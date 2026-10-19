using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;

namespace LocacaoImoveis.Domain.Tests
{
    public class ImovelTest
    {
        [Fact]
        public void DeveCriarImovelDisponivel()
        {
            var imovel = new Imovel("Rua A, 123", "Apartamento Luxo", 4, 250.00m);

            Assert.NotEqual(Guid.Empty, imovel.Id);
            Assert.Equal("Rua A, 123", imovel.Endereco);
        }

        [Theory] // diz q usa dados
        [InlineData(0)] // capacidade = 0 e -1
        [InlineData(-1)]
        public void DeveRejeitarCapacidadeDeHospedesInvalida(int capacidade)
        {
            // tenta criar um imóvel com capacidade inválida e espera que lance uma exceção
            var erro = Assert.Throws<ArgumentException>(() => 
            new Imovel("Rua A, 123", "Apartamento Luxo", capacidade, 250.00m));

            Assert.Equal("capacidadeDeHospedes", erro.ParamName); // ve se o erro de capacidade

        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void DeveRejeitarEnderecoVazio(string endereco)
        {
            var erro = Assert.Throws<ArgumentException>(() => new Imovel(
                endereco, "Casa com piscina", 6, 250m));

            Assert.Equal("endereco", erro.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void DeveRejeitarDiariaInvalida(int valor)
        {
            decimal diaria = valor;

            var erro = Assert.Throws<ArgumentException>(() => new Imovel(
                "Rua das Flores, 10", "Casa com piscina", 6, diaria));

            Assert.Equal("valorDiaria", erro.ParamName);
        }





    }
}