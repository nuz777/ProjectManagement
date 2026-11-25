using Project_Management.Domain.Enums;

namespace Project_Management.Domain.Entities;

/// <summary>
/// Tarea perteneciente a un proyecto.
/// </summary>
public sealed class Tarea
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public EstadoTarea Estado { get; set; }
    public PrioridadTarea Prioridad { get; set; }
    public DateTime FechaLimite { get; set; }
    public int ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
}
