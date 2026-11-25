using Microsoft.EntityFrameworkCore;
using Project_Management.Application.Abstractions;
using Project_Management.Domain.Entities;

namespace Project_Management.Infrastructure.Persistence;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos de gestión de proyectos.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Tarea> Tareas => Set<Tarea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
