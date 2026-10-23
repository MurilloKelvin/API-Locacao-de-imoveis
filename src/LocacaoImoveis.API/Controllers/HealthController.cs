using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LocacaoImoveis.API.Controllers;

/// <summary>Disponibiliza a consulta de saúde da API.</summary>
[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    /// <summary>Inicializa o controller com o executor dos health checks.</summary>
    /// <param name="healthCheckService">Serviço de verificação de saúde.</param>
    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>Consulta o estado dos health checks registrados na API.</summary>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">API saudável.</response>
    /// <response code="503">Um health check indicou degradação ou falha.</response>
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

/// <summary>Resultado geral da verificação de saúde.</summary>
public sealed class HealthResponse
{
    /// <summary>Estado geral: Healthy, Degraded ou Unhealthy.</summary>
    public string status { get; set; } = string.Empty;
}
