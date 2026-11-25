using Microsoft.EntityFrameworkCore;
using Project_Management.Application.Abstractions;
using Project_Management.Application.DTOs;
using Project_Management.Domain.Exceptions;
using Project_Management.Domain.Entities;
using Project_Management.Domain.Enums;

namespace Project_Management.Application.Services;

public sealed class ProyectoService(IApplicationDbContext context) : IProyectoService
{
    public async Task<IReadOnlyList<ProyectoResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Proyectos
            .AsNoTracking()
            .OrderBy(proyecto => proyecto.Id)
            .Select(proyecto => ToResponse(proyecto))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProyectoResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var proyecto = await FindByIdAsync(id, asNoTracking: true, cancellationToken);
        return ToResponse(proyecto);
    }

    public async Task<ProyectoResponse> CreateAsync(
        ProyectoRequest request,
        CancellationToken cancellationToken = default)
    {
        var fechaInicio = DateTimeNormalizer.ToUtc(request.FechaInicio);
        var fechaFin = DateTimeNormalizer.ToUtc(request.FechaFin);
        ValidateFechas(fechaInicio, fechaFin);

        var proyecto = new Proyecto
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            FechaInicio = fechaInicio,
            FechaFin = fechaFin
        };

        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync(cancellationToken);

        return ToResponse(proyecto);
    }

    public async Task<ProyectoResponse> UpdateAsync(
        int id,
        ProyectoRequest request,
        CancellationToken cancellationToken = default)
    {
        var proyecto = await FindByIdAsync(id, asNoTracking: false, cancellationToken);
        var fechaInicio = DateTimeNormalizer.ToUtc(request.FechaInicio);
        var fechaFin = DateTimeNormalizer.ToUtc(request.FechaFin);
        ValidateFechas(fechaInicio, fechaFin);

        proyecto.Nombre = request.Nombre.Trim();
        proyecto.Descripcion = request.Descripcion?.Trim();
        proyecto.FechaInicio = fechaInicio;
        proyecto.FechaFin = fechaFin;

        await context.SaveChangesAsync(cancellationToken);
        return ToResponse(proyecto);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var proyecto = await FindByIdAsync(id, asNoTracking: false, cancellationToken);

        var tieneTareasActivas = await context.Tareas
            .AnyAsync(tarea => tarea.ProyectoId == id && tarea.Estado != EstadoTarea.Completada, cancellationToken);

        if (tieneTareasActivas)
        {
            throw new BusinessRuleException(
                $"No se puede eliminar el proyecto '{proyecto.Nombre}' porque tiene tareas pendientes o en progreso. " +
                "Completa o elimina esas tareas antes de eliminar el proyecto.");
        }

        context.Proyectos.Remove(proyecto);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TareaResponse>> GetTareasAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await FindByIdAsync(id, asNoTracking: true, cancellationToken);

        return await context.Tareas
            .AsNoTracking()
            .Where(tarea => tarea.ProyectoId == id)
            .OrderBy(tarea => tarea.FechaLimite)
            .Select(tarea => ToTareaResponse(tarea))
            .ToListAsync(cancellationToken);
    }

    public async Task<AvanceResponse> GetAvanceAsync(int id, CancellationToken cancellationToken = default)
    {
        await FindByIdAsync(id, asNoTracking: true, cancellationToken);

        var totalTareas = await context.Tareas
            .CountAsync(tarea => tarea.ProyectoId == id, cancellationToken);

        if (totalTareas == 0)
        {
            return new AvanceResponse(id, 0, 0, 0);
        }

        var tareasCompletadas = await context.Tareas
            .CountAsync(
                tarea => tarea.ProyectoId == id && tarea.Estado == EstadoTarea.Completada,
                cancellationToken);

        var porcentaje = Math.Round((double)tareasCompletadas / totalTareas * 100, 2);
        return new AvanceResponse(id, totalTareas, tareasCompletadas, porcentaje);
    }

    private async Task<Proyecto> FindByIdAsync(
        int id,
        bool asNoTracking,
        CancellationToken cancellationToken)
    {
        var query = asNoTracking ? context.Proyectos.AsNoTracking() : context.Proyectos.AsQueryable();

        return await query.FirstOrDefaultAsync(proyecto => proyecto.Id == id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el proyecto con Id {id}.");
    }

    private static void ValidateFechas(DateTime fechaInicio, DateTime fechaFin)
    {
        if (fechaFin < fechaInicio)
        {
            throw new ValidationException(
                "La fecha de fin del proyecto debe ser posterior o igual a la fecha de inicio.",
                new Dictionary<string, string[]>
                {
                    ["FechaFin"] = ["Debe ser posterior o igual a FechaInicio."]
                });
        }
    }

    private static ProyectoResponse ToResponse(Proyecto proyecto) => new(
        proyecto.Id,
        proyecto.Nombre,
        proyecto.Descripcion,
        proyecto.FechaInicio,
        proyecto.FechaFin);

    private static TareaResponse ToTareaResponse(Tarea tarea) => new(
        tarea.Id,
        tarea.Titulo,
        tarea.Descripcion,
        tarea.Estado,
        tarea.Prioridad,
        tarea.FechaLimite,
        tarea.ProyectoId);
}
