namespace Project_Management.Domain.Entities;

/// <summary>
/// Proyecto que agrupa un conjunto de tareas.
/// </summary>
public sealed class Proyecto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
}
