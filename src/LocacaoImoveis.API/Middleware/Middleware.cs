using LocacaoImoveis.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LocacaoImoveis.API.Middleware;

/// <summary>Converte exceções em respostas padronizadas com ProblemDetails.</summary>
public sealed class ExceptionHandlingMiddleware
{
    // Representa o próximo componente da pipeline HTTP.
    private readonly RequestDelegate _next;

    // Registra detalhes dos erros para diagnóstico interno da aplicação.
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    /// <summary>Inicializa o tratamento global de exceções.</summary>
    /// <param name="next">Próximo componente da pipeline HTTP.</param>
    /// <param name="logger">Logger dos erros da requisição.</param>
    /// <param name="problemDetailsService">Serviço que escreve respostas ProblemDetails.</param>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IProblemDetailsService problemDetailsService)
    {
        // next continua a execucao da requisiçao o logger registra o erro e o problemDetailsService escreve a resposta padronizada
        _next = next;
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }


    /// <summary>Executa a requisição e trata exceções não capturadas.</summary>
    /// <param name="context">Contexto da requisição HTTP.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Permite que a requisição continue até os próximos middlewares e controllers.
            await _next(context);
        }
        catch (Exception exception)
        {
            // Registra a exceção sem expor seus detalhes internos ao cliente.
            _logger.LogError(
                exception,
                "Erro não tratado durante a requisição {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteProblemDetailsAsync(context, exception);
        }
    }

    // mapeia as exeçoes para os codigos http e escreve a resposta padrao com o problemadetails
    private async Task WriteProblemDetailsAsync(
        HttpContext context,
        Exception exception)
    {
        // Converte os tipos de exceção da aplicação em códigos HTTP apropriados.
        var (statusCode, title) = exception switch
        {
            CpfJaCadastradoException or EmailJaCadastradoException
                => (StatusCodes.Status409Conflict, "Conflito"),

            RecursoNaoEncontradoException
                => (StatusCodes.Status404NotFound, "Recurso não encontrado"),

            InvalidOperationException
                => (StatusCodes.Status409Conflict, "Regra de negócio violada"),

            ArgumentException
                => (StatusCodes.Status400BadRequest, "Requisição inválida"),

                //padrao
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno do servidor")
        };

        // Cria uma resposta padronizada 
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            // Mensagem generica para erros inesperados
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Ocorreu um erro inesperado."
                : exception.Message,
            // Identifica a rota em que o erro ocorreu.
            Instance = context.Request.Path
        };

        // Define o statuscode do erro
        context.Response.StatusCode = statusCode;
        if (!await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problemDetails,
                Exception = exception
            }))
        {
            // Se o serviço de ProblemDetails não conseguiu escrever a resposta, escreve manualmente.
            await context.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: "application/problem+json");
        }
    }
}
