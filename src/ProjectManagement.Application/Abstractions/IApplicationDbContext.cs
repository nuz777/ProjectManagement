using Microsoft.EntityFrameworkCore;
using Project_Management.Domain.Entities;

namespace Project_Management.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Proyecto> Proyectos { get; }
    DbSet<Tarea> Tareas { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
