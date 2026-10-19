using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;

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
