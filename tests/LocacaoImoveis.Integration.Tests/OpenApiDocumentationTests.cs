using System.Text.Json;

namespace LocacaoImoveis.Integration.Tests;

public class OpenApiDocumentationTests
{
    [Fact]
    public async Task DeveIncluirComentariosXmlNoOpenApi()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");

        var listagem = paths.GetProperty("/api/v1/reservas").GetProperty("get");
        Assert.Equal("Reservas da página solicitada; lista vazia quando não há resultados.",
            listagem.GetProperty("responses").GetProperty("200").GetProperty("description").GetString());

        var consulta = paths.GetProperty("/api/v1/reservas/{id}/detalhes").GetProperty("get");
        var id = consulta.GetProperty("parameters").EnumerateArray()
            .Single(parameter => parameter.GetProperty("name").GetString() == "id");
        Assert.Equal("Identificador da reserva.", id.GetProperty("description").GetString());
    }
}
