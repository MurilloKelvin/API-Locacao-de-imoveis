using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LocacaoImoveis.Application.DTOs.Clientes;
using LocacaoImoveis.Application.DTOs.Imoveis;

namespace LocacaoImoveis.Integration.Tests;

public class ReservaHttpTests
{
    // Mantém as mesmas opções de serialização usadas pela API nos testes HTTP.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task DeveConsultarClientesPorHttp()
    {
        // Inicializa a API em um servidor de teste e cria um cliente HTTP.
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        // Executa uma requisição real contra o endpoint de clientes.
        var response = await client.GetAsync("/api/v1/clientes/obterTodosClientes");

        // Verifica se a API respondeu com sucesso e retornou dados do seed.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var clientes = await response.Content.ReadFromJsonAsync<List<ClienteResponse>>(JsonOptions);

        Assert.NotNull(clientes);
        Assert.NotEmpty(clientes);
    }

    [Fact]
    public async Task DeveCriarReservaPorHttp()
    {
        // Cada teste possui sua própria instância da API e do banco SQLite em memória.
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        // Obtém IDs reais criados pelo seed, evitando valores fixos no teste.
        var (clienteId, imovelId) = await ObterDadosDoSeedAsync(client);

        // Cria um período futuro válido para a reserva.
        var entrada = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
        var request = new
        {
            clienteId,
            imovelId,
            dataEntrada = entrada,
            dataSaida = entrada.AddDays(2),
            quantidadeHospedes = 2
        };

        // Envia a reserva pelo endpoint HTTP, passando pelo controller e pelos serviços.
        var response = await client.PostAsJsonAsync("/api/v1/reservas", request);

        // Guarda o corpo para facilitar o diagnóstico caso a API retorne um erro.
        var responseBody = await response.Content.ReadAsStringAsync();

        // A criação deve retornar 201 e informar a localização da nova reserva.
        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            responseBody);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task DevePermitirSomenteUmaReservaConcorrentePorHttp()
    {
        // Inicializa uma API real para testar o comportamento concorrente por HTTP.
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var (clienteId, imovelId) = await ObterDadosDoSeedAsync(client);

        // As duas requisições utilizarão exatamente o mesmo imóvel e período.
        var entrada = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(20);
        var request = new
        {
            clienteId,
            imovelId,
            dataEntrada = entrada,
            dataSaida = entrada.AddDays(2),
            quantidadeHospedes = 2
        };

        // Dispara duas tentativas simultâneas de criar a mesma reserva.
        var responses = await Task.WhenAll(
            client.PostAsJsonAsync("/api/v1/reservas", request),
            client.PostAsJsonAsync("/api/v1/reservas", request));

        // Uma requisição deve criar a reserva e a outra deve ser rejeitada por conflito.
        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));
    }

    private static async Task<(Guid ClienteId, Guid ImovelId)> ObterDadosDoSeedAsync(
        HttpClient client)
    {
        // Consulta os clientes pela API e confirma que o endpoint respondeu com sucesso.
        var clientesResponse = await client.GetAsync("/api/v1/clientes/obterTodosClientes");
        clientesResponse.EnsureSuccessStatusCode();

        var clientes = await clientesResponse.Content
            .ReadFromJsonAsync<List<ClienteResponse>>(JsonOptions);

        // Consulta os imóveis pela API e confirma que o endpoint respondeu com sucesso.
        var imoveisResponse = await client.GetAsync("/api/v1/imoveis/listar");
        imoveisResponse.EnsureSuccessStatusCode();

        var imoveis = await imoveisResponse.Content
            .ReadFromJsonAsync<List<ImovelResponse>>(JsonOptions);

        // Usa os primeiros registros retornados pelo seed para montar as requisições.
        var cliente = clientes!.First();
        var imovel = imoveis!.First();

        return (cliente.Id, imovel.Id);
    }
}
