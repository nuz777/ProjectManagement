namespace Project_Management.Api.Middleware;

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Project_Management.Domain.Exceptions;

/// <summary>
/// Middleware que convierte las excepciones de la aplicación en respuestas ProblemDetails (RFC 7807).
/// </summary>
/// <param name="next">Siguiente middleware del pipeline.</param>
/// <param name="logger">Registrador de eventos.</param>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError; // Error inesperado 
        var title = "Error interno del servidor";
        var detail = "Ocurrió un error inesperado. Inténtelo de nuevo más tarde.";
        IReadOnlyDictionary<string, string[]>? errors = null;

        switch (exception)
        {
            case NotFoundException notFoundException:
                statusCode = StatusCodes.Status404NotFound;  // Recurso/item no encontrado 
                title = "Recurso no encontrado";
                detail = notFoundException.Message;
                logger.LogWarning(notFoundException, "Recurso no encontrado: {Message}", notFoundException.Message);
                break;

            case BusinessRuleException businessRuleException:
                statusCode = StatusCodes.Status409Conflict;
                title = "Conflicto con una regla de negocio";
                detail = businessRuleException.Message;
                logger.LogWarning(businessRuleException, "Conflicto de negocio: {Message}", businessRuleException.Message);
                break;

            case ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest; //  Validacion Inconrrecta 
                title = "Validación incorrecta";
                detail = validationException.Message;
                errors = validationException.Errors;
                logger.LogWarning(validationException, "Validación incorrecta: {Message}", validationException.Message);
                break;

            default:
                logger.LogError(exception, "Error no controlado al procesar {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                break;
        }

        if (context.Response.HasStarted)
        {
            logger.LogError(exception, "La respuesta ya había comenzado; no se pudo escribir ProblemDetails");
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = MediaTypeNames.Application.ProblemJson;

        IDictionary<string, object?>? extensions = null;
        if (errors is not null && errors.Count > 0)
        {
            extensions = new Dictionary<string, object?> { ["errors"] = errors };
        }

        var problem = Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            instance: context.Request.Path.Value,
            extensions: extensions);

        await problem.ExecuteAsync(context);
    }
}
