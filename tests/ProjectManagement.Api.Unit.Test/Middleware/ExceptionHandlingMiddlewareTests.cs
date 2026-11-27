using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Project_Management.Api.Middleware;
using Project_Management.Domain.Exceptions;

namespace ProjectManagement.Api.Unit.Test.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        return new DefaultHttpContext
        {
            RequestServices = services,
            Request = { Method = HttpMethods.Get, Path = "/api/v1/proyectos/1" },
            Response = { Body = new MemoryStream() }
        };
    }

    [Fact]
    public async Task InvokeAsync_SinExcepcion_EjecutaSiguienteMiddleware()
    {
        var called = false;
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ =>
            {
                called = true;
                return Task.CompletedTask;
            },
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.True(called);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ConNotFoundException_Retorna404ProblemDetails()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException("No se encontró el proyecto."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("Recurso no encontrado", doc.RootElement.GetProperty("title").GetString());
        Assert.Equal("No se encontró el proyecto.", doc.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task InvokeAsync_ConBusinessRuleException_Retorna409ProblemDetails()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new BusinessRuleException("Conflicto de negocio."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ConValidationException_Retorna400ConErrors()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ValidationException(
                "Validación fallida.",
                new Dictionary<string, string[]> { ["FechaFin"] = ["Fecha inválida."] }),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);
        var errors = doc.RootElement.GetProperty("errors");
        Assert.Equal("Fecha inválida.", errors.GetProperty("FechaFin")[0].GetString());
    }

    [Fact]
    public async Task InvokeAsync_ConExcepcionNoControlada_Retorna500ProblemDetails()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("Fallo inesperado"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }
}
