using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace LocacaoImoveis.API.OpenApi;

public sealed class OpenApiExamplesTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Identifica a rota e o metodo que serao documentados.
        var path = context.Description.RelativePath ?? string.Empty;
        var method = context.Description.HttpMethod;
        var requestContent = operation.RequestBody?.Content;
        // Adiciona exemplos de request e response para cada endpoint.
        OpenApiMediaType? mediaType = null;
        if (requestContent is not null)
            requestContent.TryGetValue("application/json", out mediaType);

        if (mediaType is not null && method == "POST" && path.EndsWith("/clientes/criarCliente", StringComparison.OrdinalIgnoreCase))
        {
            // Exemplo de request para criar cliente.
            mediaType.Example = new JsonObject
            {
                ["nome"] = "Pedro da Silva",
                ["telefone"] = "31999999999",
                ["email"] = "pedro.silva@email.com",
                ["cpf"] = "52998224725"
            };
            SetResponseExample(operation, "201", new JsonObject
            {
                ["id"] = "00000"
            });
        }
        else if (mediaType is not null && method == "POST" && path.EndsWith("/imoveis", StringComparison.OrdinalIgnoreCase))
        {
            // Exemplo de request para criar imovel.
            mediaType.Example = new JsonObject
            {
                ["endereco"] = "Rua das Flores, 100",
                ["titulo"] = "Casa das Flores",
                ["capacidadeDeHospedes"] = 4,
                ["valorDiaria"] = 250m
            };
            SetResponseExample(operation, "201", new JsonObject
            {
                ["id"] = "55555"
            });
        }
        else if (mediaType is not null && method == "POST" && path.EndsWith("/reservas", StringComparison.OrdinalIgnoreCase))
        {
            // Exemplo de request para criar reserva.
            mediaType.Example = new JsonObject
            {
                ["clienteId"] = "111111",
                ["imovelId"] = "2222222",
                ["dataEntrada"] = "2030-01-10",
                ["dataSaida"] = "2030-01-13",
                ["quantidadeHospedes"] = 2
            };
            SetResponseExample(operation, "201", new JsonObject
            {
                ["id"] = "33333"
            });
        }
        else if (method == "GET" && path.Equals("health", StringComparison.OrdinalIgnoreCase))
        {
            // Exemplo de resposta do health check.
            SetResponseExample(operation, "200", new JsonObject
            {
                ["status"] = "Healthy"
            });
        }

        return Task.CompletedTask;
    }

    private static void SetResponseExample(
        OpenApiOperation operation,
        string statusCode,
        JsonNode example)
    {
        // Adiciona o exemplo na resposta JSON quando ela existir.
        var responses = operation.Responses;
        if (responses is null || !responses.TryGetValue(statusCode, out var response))
            return;

        var responseContent = response.Content;
        if (responseContent is null ||
            !responseContent.TryGetValue("application/json", out var mediaType) ||
            mediaType is null)
            return;

        mediaType.Example = example;
    }
}
