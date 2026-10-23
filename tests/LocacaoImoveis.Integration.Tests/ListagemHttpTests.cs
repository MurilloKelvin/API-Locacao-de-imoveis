using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.DTOs.Reservas;

namespace LocacaoImoveis.Integration.Tests;

public class ListagemHttpTests
{
    [Theory]
    [InlineData("/api/v1/clientes/obterTodosClientes")]
    [InlineData("/api/v1/reservas")]
    public async Task DevePaginarSemRepetirRegistrosERetornarPaginaVazia(string rota)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var todos = await client.GetFromJsonAsync<JsonElement[]>($"{rota}?TamanhoPagina=100");
        Assert.True(todos!.Length >= 2);

        for (var pagina = 1; pagina <= todos.Length; pagina++)
        {
            var itens = await client.GetFromJsonAsync<JsonElement[]>($"{rota}?Pagina={pagina}&TamanhoPagina=1");
            Assert.Single(itens!);
            Assert.Equal(todos[pagina - 1].GetProperty("id").GetGuid(), itens![0].GetProperty("id").GetGuid());
        }

        var vazia = await client.GetFromJsonAsync<JsonElement[]>($"{rota}?Pagina={todos.Length + 1}&TamanhoPagina=1");
        Assert.Empty(vazia!);
    }

    [Theory]
    [InlineData("/api/v1/clientes/obterTodosClientes")]
    [InlineData("/api/v1/reservas")]
    public async Task DeveRejeitarPaginacaoEStatusInvalidos(string rota)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        foreach (var query in new[] { "Pagina=0", "Pagina=-1", "TamanhoPagina=0", "TamanhoPagina=101", "Status=999", "Pagina=2147483647&TamanhoPagina=100" })
        {
            var response = await client.GetAsync($"{rota}?{query}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task DeveFiltrarClientesPorNomeEStatusAntesDePaginar()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        const string rota = "/api/v1/clientes/obterTodosClientes";
        var todos = (await client.GetFromJsonAsync<List<ClienteResponse>>($"{rota}?TamanhoPagina=100"))!;
        var alvo = todos.Last();
        var nome = alvo.Nome[..3];
        var query = $"Nome={Uri.EscapeDataString(" " + nome.ToUpperInvariant() + " ")}&Status={(int)alvo.Status}";
        var filtrados = await client.GetFromJsonAsync<List<ClienteResponse>>($"{rota}?{query}&TamanhoPagina=1");
        var esperado = todos.First(c => c.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase) && c.Status == alvo.Status);
        Assert.Equal(esperado.Id, Assert.Single(filtrados!).Id);

        var inexistentes = await client.GetFromJsonAsync<List<ClienteResponse>>($"{rota}?Nome=nome-inexistente-xyz");
        Assert.Empty(inexistentes!);
        var outroStatus = await client.GetFromJsonAsync<List<ClienteResponse>>($"{rota}?Nome={Uri.EscapeDataString(alvo.Nome)}&Status={1 - (int)alvo.Status}");
        Assert.DoesNotContain(outroStatus!, c => c.Id == alvo.Id);
    }

    [Fact]
    public async Task DeveCombinarFiltrosDeReservaAntesDePaginar()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        const string rota = "/api/v1/reservas";
        var todas = (await client.GetFromJsonAsync<List<ReservaResponse>>($"{rota}?TamanhoPagina=100"))!;
        var alvo = todas.Last();
        var query = $"ClienteId={alvo.ClienteId}&ImovelId={alvo.ImovelId}&Status={(int)alvo.Status}";
        var filtradas = await client.GetFromJsonAsync<List<ReservaResponse>>($"{rota}?{query}&TamanhoPagina=1");
        var esperada = todas.First(r => r.ClienteId == alvo.ClienteId && r.ImovelId == alvo.ImovelId && r.Status == alvo.Status);
        Assert.Equal(esperada.Id, Assert.Single(filtradas!).Id);

        foreach (var filtro in new[] { $"ClienteId={Guid.NewGuid()}", $"ImovelId={Guid.NewGuid()}" })
            Assert.Empty((await client.GetFromJsonAsync<List<ReservaResponse>>($"{rota}?{filtro}"))!);

        var outroStatus = await client.GetFromJsonAsync<List<ReservaResponse>>($"{rota}?Status={((int)alvo.Status + 1) % 4}");
        Assert.DoesNotContain(outroStatus!, r => r.Id == alvo.Id);
    }
}
