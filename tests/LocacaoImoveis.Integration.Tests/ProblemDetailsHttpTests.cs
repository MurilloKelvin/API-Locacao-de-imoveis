using System.Net;
using System.Text;
using System.Text.Json;

namespace LocacaoImoveis.Integration.Tests;

public class ProblemDetailsHttpTests
{
    [Theory]
    [InlineData("/api/v1/clientes/123/detalhes", HttpStatusCode.BadRequest)]
    [InlineData("/api/v1/clientes/00000000-0000-0000-0000-000000000000/detalhes", HttpStatusCode.NotFound)]
    [InlineData("/rota-inexistente", HttpStatusCode.NotFound)]
    public async Task DeveRetornarProblemDetailsParaErrosHttp(string rota, HttpStatusCode statusEsperado)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(rota);

        Assert.Equal(statusEsperado, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var problem = json.RootElement;
        Assert.Equal((int)statusEsperado, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("title").GetString()));
        Assert.Equal(rota, problem.GetProperty("instance").GetString());
    }

    [Fact]
    public async Task DeveRetornarProblemDetailsParaJsonInvalido()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        using var content = new StringContent("{", Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/clientes/criarCliente", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var problem = json.RootElement;
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("title").GetString()));
        Assert.True(problem.TryGetProperty("errors", out _));
    }
}
