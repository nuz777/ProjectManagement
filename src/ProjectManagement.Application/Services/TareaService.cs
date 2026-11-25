using Microsoft.EntityFrameworkCore;
using Project_Management.Application.Abstractions;
using Project_Management.Application.DTOs;
using Project_Management.Domain.Exceptions;
using Project_Management.Domain.Entities;
using Project_Management.Domain.Enums;

namespace Project_Management.Application.Services;

public sealed class TareaService(
    IApplicationDbContext context,
    TimeProvider timeProvider) : ITareaService
{
    public async Task<IReadOnlyList<TareaResponse>> GetAllAsync(
        EstadoTarea? estado,
        PrioridadTarea? prioridad,
        CancellationToken cancellationToken = default)
    {
        ValidateFiltros(estado, prioridad);

        var query = context.Tareas.AsNoTracking().AsQueryable();

        if (estado.HasValue)
        {
            query = query.Where(tarea => tarea.Estado == estado.Value);
        }

        if (prioridad.HasValue)
        {
            query = query.Where(tarea => tarea.Prioridad == prioridad.Value);
        }

        return await query
            .OrderBy(tarea => tarea.FechaLimite)
            .Select(tarea => ToResponse(tarea))
            .ToListAsync(cancellationToken);
    }

    public async Task<TareaResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tarea = await FindByIdAsync(id, asNoTracking: true, cancellationToken);
        return ToResponse(tarea);
    }

    public async Task<TareaResponse> CreateAsync(
        TareaRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateEnums(request.Estado, request.Prioridad);
        var proyecto = await FindProyectoAsync(request.ProyectoId, cancellationToken);
        var fechaLimite = DateTimeNormalizer.ToUtc(request.FechaLimite);
        ValidateFechaLimite(fechaLimite, proyecto.FechaInicio);

        var tarea = new Tarea
        {
            Titulo = request.Titulo.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            Estado = request.Estado,
            Prioridad = request.Prioridad,
            FechaLimite = fechaLimite,
            ProyectoId = request.ProyectoId
        };

        context.Tareas.Add(tarea);
        await context.SaveChangesAsync(cancellationToken);

        return ToResponse(tarea);
    }

    public async Task<TareaResponse> UpdateAsync(
        int id,
        TareaRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateEnums(request.Estado, request.Prioridad);
        var tarea = await FindByIdAsync(id, asNoTracking: false, cancellationToken);
        var proyecto = await FindProyectoAsync(request.ProyectoId, cancellationToken);
        var fechaLimite = DateTimeNormalizer.ToUtc(request.FechaLimite);
        ValidateFechaLimite(fechaLimite, proyecto.FechaInicio);

        tarea.Titulo = request.Titulo.Trim();
        tarea.Descripcion = request.Descripcion?.Trim();
        tarea.Estado = request.Estado;
        tarea.Prioridad = request.Prioridad;
        tarea.FechaLimite = fechaLimite;
        tarea.ProyectoId = request.ProyectoId;

        await context.SaveChangesAsync(cancellationToken);
        return ToResponse(tarea);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var tarea = await FindByIdAsync(id, asNoTracking: false, cancellationToken);
        context.Tareas.Remove(tarea);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TareaResponse>> GetVencidasAsync(
        CancellationToken cancellationToken = default)
    {
        var ahora = timeProvider.GetUtcNow().UtcDateTime;

        return await context.Tareas
            .AsNoTracking()
            .Where(tarea => tarea.FechaLimite < ahora && tarea.Estado != EstadoTarea.Completada)
            .OrderBy(tarea => tarea.FechaLimite)
            .Select(tarea => ToResponse(tarea))
            .ToListAsync(cancellationToken);
    }

    public async Task<TareaResponse> UpdateEstadoAsync(
        int id,
        ActualizarEstadoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.Estado))
        {
            throw new ValidationException(
                "El estado indicado para la tarea no es válido.",
                new Dictionary<string, string[]> { ["Estado"] = ["Debe ser Pendiente, EnProgreso o Completada."] });
        }

        var tarea = await FindByIdAsync(id, asNoTracking: false, cancellationToken);
        tarea.Estado = request.Estado;

        await context.SaveChangesAsync(cancellationToken);
        return ToResponse(tarea);
    }

    private async Task<Tarea> FindByIdAsync(
        int id,
        bool asNoTracking,
        CancellationToken cancellationToken)
    {
        var query = asNoTracking ? context.Tareas.AsNoTracking() : context.Tareas.AsQueryable();

        return await query.FirstOrDefaultAsync(tarea => tarea.Id == id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró la tarea con Id {id}.");
    }

    private async Task<Proyecto> FindProyectoAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Proyectos
            .AsNoTracking()
            .FirstOrDefaultAsync(proyecto => proyecto.Id == id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el proyecto con Id {id} indicado para la tarea.");
    }

    private static void ValidateFiltros(EstadoTarea? estado, PrioridadTarea? prioridad)
    {
        if (estado.HasValue && !Enum.IsDefined(estado.Value))
        {
            throw new ValidationException(
                "El filtro de estado no es válido.",
                new Dictionary<string, string[]> { ["estado"] = ["Debe ser Pendiente, EnProgreso o Completada."] });
        }

        if (prioridad.HasValue && !Enum.IsDefined(prioridad.Value))
        {
            throw new ValidationException(
                "El filtro de prioridad no es válido.",
                new Dictionary<string, string[]> { ["prioridad"] = ["Debe ser Baja, Media o Alta."] });
        }
    }

    private static void ValidateEnums(EstadoTarea estado, PrioridadTarea prioridad)
    {
        var errores = new Dictionary<string, string[]>();

        if (!Enum.IsDefined(estado))
        {
            errores["Estado"] = ["Debe ser Pendiente, EnProgreso o Completada."];
        }

        if (!Enum.IsDefined(prioridad))
        {
            errores["Prioridad"] = ["Debe ser Baja, Media o Alta."];
        }

        if (errores.Count > 0)
        {
            throw new ValidationException("Uno o más valores de enumeración no son válidos.", errores);
        }
    }

    private static void ValidateFechaLimite(DateTime fechaLimite, DateTime fechaInicioProyecto)
    {
        if (fechaLimite < fechaInicioProyecto)
        {
            throw new ValidationException(
                "La fecha límite de la tarea no puede ser anterior a la fecha de inicio del proyecto.",
                new Dictionary<string, string[]>
                {
                    ["FechaLimite"] = ["Debe ser posterior o igual a la fecha de inicio del proyecto."]
                });
        }
    }

    private static TareaResponse ToResponse(Tarea tarea) => new(
        tarea.Id,
        tarea.Titulo,
        tarea.Descripcion,
        tarea.Estado,
        tarea.Prioridad,
        tarea.FechaLimite,
        tarea.ProyectoId);
}
