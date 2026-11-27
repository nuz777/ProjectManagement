using Project_Management.Application.DTOs;
using Project_Management.Application.Services;
using ProjectManagement.Application.Unit.Test.TestDoubles;
using Project_Management.Domain.Entities;
using Project_Management.Domain.Enums;
using Project_Management.Domain.Exceptions;

namespace ProjectManagement.Application.Unit.Test.Services;

public sealed class TareaServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAllAsync_ConFiltrosEstadoYPrioridad_RetornaCoincidenciasOrdenadasPorFechaLimite()
    {
        await using var context = TestApplicationDbContext.Create();
        context.Tareas.AddRange(
            new Tarea { Titulo = "Tardía", Estado = EstadoTarea.Pendiente, Prioridad = PrioridadTarea.Alta, FechaLimite = Now.UtcDateTime.AddDays(5), ProyectoId = 1 },
            new Tarea { Titulo = "Temprana", Estado = EstadoTarea.Pendiente, Prioridad = PrioridadTarea.Alta, FechaLimite = Now.UtcDateTime.AddDays(1), ProyectoId = 1 },
            new Tarea { Titulo = "Otra prioridad", Estado = EstadoTarea.Pendiente, Prioridad = PrioridadTarea.Baja, FechaLimite = Now.UtcDateTime.AddDays(1), ProyectoId = 1 });
        await context.SaveChangesAsync();

        var service = new TareaService(context, new FixedTimeProvider(Now));
        var result = await service.GetAllAsync(EstadoTarea.Pendiente, PrioridadTarea.Alta);

        Assert.Equal(2, result.Count);
        Assert.Equal("Temprana", result[0].Titulo);
        Assert.Equal("Tardía", result[1].Titulo);
    }

    [Fact]
    public async Task GetAllAsync_ConEstadoInvalido_LanzaValidationException()
    {
        await using var context = TestApplicationDbContext.Create();
        var service = new TareaService(context, new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.GetAllAsync((EstadoTarea)999, null));
    }

    [Fact]
    public async Task CreateAsync_CuandoProyectoNoExiste_LanzaNotFoundException()
    {
        await using var context = TestApplicationDbContext.Create();
        var service = new TareaService(context, new FixedTimeProvider(Now));
        var request = new TareaRequest
        {
            Titulo = "Sin proyecto",
            Estado = EstadoTarea.Pendiente,
            Prioridad = PrioridadTarea.Media,
            FechaLimite = Now.UtcDateTime.AddDays(2),
            ProyectoId = 404
        };

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_CuandoFechaLimiteEsAnteriorAInicioProyecto_LanzaValidationException()
    {
        await using var context = TestApplicationDbContext.Create();
        var proyecto = new Proyecto
        {
            Nombre = "Proyecto Base",
            FechaInicio = Now.UtcDateTime,
            FechaFin = Now.UtcDateTime.AddDays(10)
        };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        var service = new TareaService(context, new FixedTimeProvider(Now));
        var request = new TareaRequest
        {
            Titulo = "Fuera de rango",
            Estado = EstadoTarea.Pendiente,
            Prioridad = PrioridadTarea.Media,
            FechaLimite = Now.UtcDateTime.AddDays(-1),
            ProyectoId = proyecto.Id
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.True(exception.Errors.ContainsKey("FechaLimite"));
    }

    [Fact]
    public async Task GetVencidasAsync_RetornaSoloVencidasNoCompletadas()
    {
        await using var context = TestApplicationDbContext.Create();
        context.Tareas.AddRange(
            new Tarea { Titulo = "Vencida Pendiente", Estado = EstadoTarea.Pendiente, Prioridad = PrioridadTarea.Alta, FechaLimite = Now.UtcDateTime.AddHours(-2), ProyectoId = 1 },
            new Tarea { Titulo = "Vencida Completada", Estado = EstadoTarea.Completada, Prioridad = PrioridadTarea.Alta, FechaLimite = Now.UtcDateTime.AddHours(-2), ProyectoId = 1 },
            new Tarea { Titulo = "Vigente", Estado = EstadoTarea.EnProgreso, Prioridad = PrioridadTarea.Media, FechaLimite = Now.UtcDateTime.AddHours(2), ProyectoId = 1 });
        await context.SaveChangesAsync();

        var service = new TareaService(context, new FixedTimeProvider(Now));
        var vencidas = await service.GetVencidasAsync();

        var tarea = Assert.Single(vencidas);
        Assert.Equal("Vencida Pendiente", tarea.Titulo);
    }

    [Fact]
    public async Task UpdateEstadoAsync_ConEstadoValido_ActualizaEstado()
    {
        await using var context = TestApplicationDbContext.Create();
        var tarea = new Tarea
        {
            Titulo = "Cambiar estado",
            Estado = EstadoTarea.Pendiente,
            Prioridad = PrioridadTarea.Media,
            FechaLimite = Now.UtcDateTime.AddDays(1),
            ProyectoId = 1
        };
        context.Tareas.Add(tarea);
        await context.SaveChangesAsync();

        var service = new TareaService(context, new FixedTimeProvider(Now));
        var updated = await service.UpdateEstadoAsync(tarea.Id, new ActualizarEstadoRequest { Estado = EstadoTarea.Completada });

        Assert.Equal(EstadoTarea.Completada, updated.Estado);
    }

    [Fact]
    public async Task UpdateEstadoAsync_ConEstadoInvalido_LanzaValidationException()
    {
        await using var context = TestApplicationDbContext.Create();
        var service = new TareaService(context, new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateEstadoAsync(1, new ActualizarEstadoRequest { Estado = (EstadoTarea)999 }));
    }
}
