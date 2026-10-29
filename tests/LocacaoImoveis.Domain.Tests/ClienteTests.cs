using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;
using LocacaoImoveis.Domain.ValueObjects;

namespace LocacaoImoveis.Domain.Tests;

public class ClienteTests
{
    [Fact]
    public void DeveCriarClienteAtivo()
    {
        var cliente = new Cliente(
            "Ana", "31999999999", "ana@email.com", "52998224725");

        Assert.Equal(ClienteStatus.Ativo, cliente.Status);
    }

    [Fact]
    public void DeveCriarClienteComCnpj()
    {
        var cliente = new Cliente(
            "Empresa Exemplo", "31999999999",
            "contato@empresa.com", "00.000.000/E08G-12");

        Assert.Equal("00000000E08G12", cliente.DocumentoCliente.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("319999999")]
    [InlineData("319999999999")]
    [InlineData("abcdefghijk")]
    public void DeveRejeitarTelefoneInvalidoAoCriarCliente(string telefone)
    {
        var erro = Assert.Throws<ArgumentException>(() => new Cliente(
            "Ana", telefone, "ana@email.com", "52998224725"));

        Assert.Equal("telefone", erro.ParamName);
    }

    [Theory]
    [InlineData("3199999999")]
    [InlineData("31999999999")]
    public void DeveAceitarTelefoneComDezOuOnzeDigitos(string telefone)
    {
        var cliente = new Cliente(
            "Ana", telefone, "ana@email.com", "52998224725");

        Assert.Equal(telefone, cliente.Telefone);
    }

    [Fact]
    public void DeveRejeitarTelefoneInvalidoAoAtualizarCliente()
    {
        var cliente = new Cliente(
            "Ana", "31999999999", "ana@email.com", "52998224725");

        var erro = Assert.Throws<ArgumentException>(() =>
            cliente.AtualizarDadosCliente("Ana Silva", "12345"));

        Assert.Equal("telefone", erro.ParamName);
        Assert.Equal("31999999999", cliente.Telefone);
    }

    [Fact]
    public void DeveInativarCliente()
    {
        var cliente = new Cliente(
            "Ana", "31999999999", "ana@email.com", "52998224725");

        cliente.InativarCliente();

        Assert.Equal(ClienteStatus.Inativo, cliente.Status);
    }

    [Fact]
    public void DeveReativarClienteInativo()
    {
        var cliente = new Cliente(
            "Ana", "31999999999", "ana@email.com", "52998224725");
        cliente.InativarCliente();

        cliente.AtivarCliente();

        Assert.Equal(ClienteStatus.Ativo, cliente.Status);
    }
}
