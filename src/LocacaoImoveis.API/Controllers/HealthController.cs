using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LocacaoImoveis.API.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    [EndpointSummary("Verifica a saude da API")]
        [EndpointDescription("Verifica se a API esta respondendo.")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        // Executa os health checks registrados.
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

        // Monta uma resposta com o resultado geral e de cada check.
        var body = new HealthResponse
        {
            status = report.Status.ToString(),
        };

        // Retorna 200 para API saudavel ou 503 para falha.
        return report.Status == HealthStatus.Healthy
            ? Ok(body)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, body);
    }
}

public sealed class HealthResponse
{
    public string status { get; set; } = string.Empty;
}
