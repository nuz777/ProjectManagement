using Project_Management.Application.DTOs;
using Project_Management.Application.Services;
using ProjectManagement.Application.Tests.TestDoubles;
using Project_Management.Domain.Entities;
using Project_Management.Domain.Enums;
using Project_Management.Domain.Exceptions;

namespace ProjectManagement.Application.Tests.Services;

public sealed class ProyectoServiceTests
{
    [Fact]
    public async Task CreateAsync_ConDatosValidos_NormalizaFechasAYGuardaProyecto()
    {
        await using var context = TestApplicationDbContext.Create();
        var service = new ProyectoService(context);
        var request = new ProyectoRequest
        {
            Nombre = "  Proyecto Alpha  ",
            Descripcion = "  Descripción inicial  ",
            FechaInicio = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Unspecified),
            FechaFin = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Unspecified)
        };

        var result = await service.CreateAsync(request);

        Assert.True(result.Id > 0);
        Assert.Equal("Proyecto Alpha", result.Nombre);
        Assert.Equal("Descripción inicial", result.Descripcion);
        Assert.Equal(DateTimeKind.Utc, result.FechaInicio.Kind);
        Assert.Equal(DateTimeKind.Utc, result.FechaFin.Kind);
        Assert.Equal(1, context.Proyectos.Count());
    }

    [Fact]
    public async Task CreateAsync_CuandoFechaFinEsAnteriorAInicio_LanzaValidationException()
    {
        await using var context = TestApplicationDbContext.Create();
        var service = new ProyectoService(context);
        var request = new ProyectoRequest
        {
            Nombre = "Proyecto Inválido",
            FechaInicio = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            FechaFin = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.True(exception.Errors.ContainsKey("FechaFin"));
    }

    [Fact]
    public async Task GetByIdAsync_CuandoNoExiste_LanzaNotFoundException()
    {
        await using var context = TestApplicationDbContext.Create();
        var service = new ProyectoService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_CuandoTieneTareasActivas_LanzaBusinessRuleException()
    {
        await using var context = TestApplicationDbContext.Create();
        var proyecto = new Proyecto
        {
            Nombre = "Con Pendientes",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(5)
        };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        context.Tareas.Add(new Tarea
        {
            Titulo = "Tarea activa",
            Estado = EstadoTarea.EnProgreso,
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(2),
            ProyectoId = proyecto.Id
        });
        await context.SaveChangesAsync();

        var service = new ProyectoService(context);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DeleteAsync(proyecto.Id));
    }

    [Fact]
    public async Task DeleteAsync_CuandoSoloTieneTareasCompletadas_EliminaProyecto()
    {
        await using var context = TestApplicationDbContext.Create();
        var proyecto = new Proyecto
        {
            Nombre = "Cerrado",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(5)
        };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        context.Tareas.Add(new Tarea
        {
            Titulo = "Tarea lista",
            Estado = EstadoTarea.Completada,
            Prioridad = PrioridadTarea.Media,
            FechaLimite = DateTime.UtcNow.AddDays(1),
            ProyectoId = proyecto.Id
        });
        await context.SaveChangesAsync();

        var service = new ProyectoService(context);
        await service.DeleteAsync(proyecto.Id);

        Assert.Empty(context.Proyectos);
    }

    [Fact]
    public async Task GetAvanceAsync_SinTareas_RetornaCeroPorCiento()
    {
        await using var context = TestApplicationDbContext.Create();
        var proyecto = new Proyecto
        {
            Nombre = "Vacío",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(5)
        };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        var service = new ProyectoService(context);
        var avance = await service.GetAvanceAsync(proyecto.Id);

        Assert.Equal(proyecto.Id, avance.ProyectoId);
        Assert.Equal(0, avance.TotalTareas);
        Assert.Equal(0, avance.TareasCompletadas);
        Assert.Equal(0, avance.Porcentaje);
    }

    [Fact]
    public async Task GetAvanceAsync_ConTareasMixtas_CalculaPorcentajeCorrecto()
    {
        await using var context = TestApplicationDbContext.Create();
        var proyecto = new Proyecto
        {
            Nombre = "Mixto",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(10)
        };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        context.Tareas.AddRange(
            new Tarea { Titulo = "T1", Estado = EstadoTarea.Completada, Prioridad = PrioridadTarea.Baja, FechaLimite = DateTime.UtcNow.AddDays(1), ProyectoId = proyecto.Id },
            new Tarea { Titulo = "T2", Estado = EstadoTarea.Pendiente, Prioridad = PrioridadTarea.Media, FechaLimite = DateTime.UtcNow.AddDays(2), ProyectoId = proyecto.Id },
            new Tarea { Titulo = "T3", Estado = EstadoTarea.EnProgreso, Prioridad = PrioridadTarea.Alta, FechaLimite = DateTime.UtcNow.AddDays(3), ProyectoId = proyecto.Id });
        await context.SaveChangesAsync();

        var service = new ProyectoService(context);
        var avance = await service.GetAvanceAsync(proyecto.Id);

        Assert.Equal(3, avance.TotalTareas);
        Assert.Equal(1, avance.TareasCompletadas);
        Assert.Equal(33.33, avance.Porcentaje);
    }
}
