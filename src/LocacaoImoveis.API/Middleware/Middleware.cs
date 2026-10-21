using LocacaoImoveis.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LocacaoImoveis.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    // Representa o próximo componente da pipeline HTTP.
    private readonly RequestDelegate _next;

    // Registra detalhes dos erros para diagnóstico interno da aplicação.
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }


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

    private static async Task WriteProblemDetailsAsync(
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
            // Não expõe a mensagem real de erros internos ao consumidor da API.
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Ocorreu um erro inesperado."
                : exception.Message,
            // Identifica a rota em que o erro ocorreu.
            Instance = context.Request.Path
        };

        // Define o status e o tipo de conteúdo da resposta HTTP.
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        // Serializa o ProblemDetails como JSON no corpo da resposta.
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}