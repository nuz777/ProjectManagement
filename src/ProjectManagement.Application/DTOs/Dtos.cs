using System.ComponentModel.DataAnnotations;
using Project_Management.Domain.Enums;

namespace Project_Management.Application.DTOs;

/// <summary>
/// Datos necesarios para crear o actualizar un proyecto.
/// </summary>
public sealed class ProyectoRequest
{
    /// <summary>Nombre del proyecto.</summary>
    [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción opcional del proyecto.</summary>
    [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>Fecha de inicio del proyecto.</summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>Fecha de finalización del proyecto. Debe ser posterior o igual a la fecha de inicio.</summary>
    public DateTime FechaFin { get; set; }
}

/// <summary>
/// Representación de un proyecto devuelta por la API.
/// </summary>
/// <param name="Id">Identificador del proyecto.</param>
/// <param name="Nombre">Nombre del proyecto.</param>
/// <param name="Descripcion">Descripción del proyecto.</param>
/// <param name="FechaInicio">Fecha de inicio.</param>
/// <param name="FechaFin">Fecha de finalización.</param>
public sealed record ProyectoResponse(
    int Id,
    string Nombre,
    string? Descripcion,
    DateTime FechaInicio,
    DateTime FechaFin);

/// <summary>
/// Datos necesarios para crear o actualizar una tarea.
/// </summary>
public sealed class TareaRequest
{
    /// <summary>Título de la tarea.</summary>
    [Required(ErrorMessage = "El título de la tarea es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Descripción opcional de la tarea.</summary>
    [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>Estado de la tarea.</summary>
    [EnumDataType(typeof(EstadoTarea), ErrorMessage = "El estado de la tarea no es válido.")]
    public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;

    /// <summary>Prioridad de la tarea.</summary>
    [EnumDataType(typeof(PrioridadTarea), ErrorMessage = "La prioridad de la tarea no es válida.")]
    public PrioridadTarea Prioridad { get; set; } = PrioridadTarea.Media;

    /// <summary>Fecha límite de la tarea. No puede ser anterior al inicio del proyecto.</summary>
    public DateTime FechaLimite { get; set; }

    /// <summary>Identificador del proyecto al que pertenece la tarea.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del proyecto es obligatorio.")]
    public int ProyectoId { get; set; }
}

/// <summary>
/// Representación de una tarea devuelta por la API.
/// </summary>
/// <param name="Id">Identificador de la tarea.</param>
/// <param name="Titulo">Título de la tarea.</param>
/// <param name="Descripcion">Descripción de la tarea.</param>
/// <param name="Estado">Estado actual de la tarea.</param>
/// <param name="Prioridad">Prioridad de la tarea.</param>
/// <param name="FechaLimite">Fecha límite de la tarea.</param>
/// <param name="ProyectoId">Identificador del proyecto propietario.</param>
public sealed record TareaResponse(
    int Id,
    string Titulo,
    string? Descripcion,
    EstadoTarea Estado,
    PrioridadTarea Prioridad,
    DateTime FechaLimite,
    int ProyectoId);

/// <summary>
/// Datos para cambiar el estado de una tarea.
/// </summary>
public sealed class ActualizarEstadoRequest
{
    /// <summary>Nuevo estado de la tarea.</summary>
    [EnumDataType(typeof(EstadoTarea), ErrorMessage = "El estado de la tarea no es válido.")]
    public EstadoTarea Estado { get; set; }
}

/// <summary>
/// Porcentaje de avance de un proyecto.
/// </summary>
/// <param name="ProyectoId">Identificador del proyecto.</param>
/// <param name="TotalTareas">Número total de tareas.</param>
/// <param name="Porcentaje">Porcentaje de tareas completadas (0-100).</param>
public sealed record AvanceResponse(
    int ProyectoId,
    int TotalTareas,
    int TareasCompletadas,
    double Porcentaje);
