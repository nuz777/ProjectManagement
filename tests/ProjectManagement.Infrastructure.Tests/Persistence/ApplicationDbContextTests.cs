using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Project_Management.Domain.Entities;
using Project_Management.Domain.Enums;
using Project_Management.Infrastructure.Persistence;

namespace ProjectManagement.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextTests
{
    private static (ApplicationDbContext Context, SqliteConnection Connection) CreateSqliteContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }

    [Fact]
    public void OnModelCreating_ConfiguraTablasRestriccionesEIndices()
    {
        var (context, connection) = CreateSqliteContext();
        using (connection)
        using (context)
        {
            var proyectoEntity = context.Model.FindEntityType(typeof(Proyecto));
            var tareaEntity = context.Model.FindEntityType(typeof(Tarea));

            Assert.NotNull(proyectoEntity);
            Assert.NotNull(tareaEntity);
            Assert.Equal("Proyectos", proyectoEntity.GetTableName());
            Assert.Equal("Tareas", tareaEntity.GetTableName());
            Assert.Equal(100, proyectoEntity.FindProperty(nameof(Proyecto.Nombre))?.GetMaxLength());
            Assert.Equal(150, tareaEntity.FindProperty(nameof(Tarea.Titulo))?.GetMaxLength());
            Assert.Equal(20, tareaEntity.FindProperty(nameof(Tarea.Estado))?.GetMaxLength());
            Assert.Equal(10, tareaEntity.FindProperty(nameof(Tarea.Prioridad))?.GetMaxLength());

            var fk = Assert.Single(tareaEntity.GetForeignKeys());
            Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
            Assert.Contains(tareaEntity.GetIndexes(), i => i.Properties.Select(p => p.Name).SequenceEqual(["ProyectoId"]));
            Assert.Contains(tareaEntity.GetIndexes(), i => i.Properties.Select(p => p.Name).SequenceEqual(["Estado", "FechaLimite"]));
        }
    }

    [Fact]
    public async Task SaveChangesAsync_AlGuardarProyectoYTarea_PersisteRelacionCorrectamente()
    {
        var (context, connection) = CreateSqliteContext();
        await using (connection)
        await using (context)
        {
            var proyecto = new Proyecto
            {
                Nombre = "Infraestructura",
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(7),
                Tareas =
                [
                    new Tarea
                    {
                        Titulo = "Configurar DB",
                        Estado = EstadoTarea.EnProgreso,
                        Prioridad = PrioridadTarea.Alta,
                        FechaLimite = DateTime.UtcNow.AddDays(2)
                    }
                ]
            };

            context.Proyectos.Add(proyecto);
            await context.SaveChangesAsync();

            var guardado = await context.Proyectos
                .Include(p => p.Tareas)
                .SingleAsync(p => p.Id == proyecto.Id);

            var tarea = Assert.Single(guardado.Tareas);
            Assert.Equal("Configurar DB", tarea.Titulo);
            Assert.Equal(proyecto.Id, tarea.ProyectoId);
        }
    }

    [Fact]
    public async Task SaveChangesAsync_AlEliminarProyecto_EliminaTareasEnCascada()
    {
        var (context, connection) = CreateSqliteContext();
        await using (connection)
        await using (context)
        {
            var proyecto = new Proyecto
            {
                Nombre = "Temporal",
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddDays(3),
                Tareas =
                [
                    new Tarea
                    {
                        Titulo = "Hija",
                        Estado = EstadoTarea.Pendiente,
                        Prioridad = PrioridadTarea.Baja,
                        FechaLimite = DateTime.UtcNow.AddDays(1)
                    }
                ]
            };

            context.Proyectos.Add(proyecto);
            await context.SaveChangesAsync();

            context.Proyectos.Remove(proyecto);
            await context.SaveChangesAsync();

            Assert.Empty(context.Proyectos);
            Assert.Empty(context.Tareas);
        }
    }
}
