using System.Net;
using System.Net.Http.Json;
using Project_Management.Application.DTOs;
using Project_Management.Domain.Enums;

namespace ProjectManagement.Api.IntegrationTests;

public sealed class TareasEndpointsTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<ProyectoResponse> CrearProyectoAsync(DateTime? fechaInicio = null)
    {
        var inicio = fechaInicio ?? DateTime.UtcNow.AddDays(-5);
        var response = await _client.PostAsJsonAsync("/api/v1/proyectos", new
        {
            Nombre = $"Proyecto {Guid.NewGuid():N}",
            FechaInicio = inicio,
            FechaFin = inicio.AddDays(20)
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProyectoResponse>(
            CustomWebApplicationFactory.JsonOptions))!;
    }

    [Fact]
    public async Task PostYGetAllConFiltros_Retorna201YFiltraPorEstadoYPrioridad()
    {
        var proyecto = await CrearProyectoAsync();
        var tituloUnico = $"Filtrada-{Guid.NewGuid():N}";

        var postRes = await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            Titulo = tituloUnico,
            Estado = "EnProgreso",
            Prioridad = "Alta",
            FechaLimite = DateTime.UtcNow.AddDays(3),
            ProyectoId = proyecto.Id
        });

        Assert.Equal(HttpStatusCode.Created, postRes.StatusCode);

        var getRes = await _client.GetAsync("/api/v1/tareas?estado=EnProgreso&prioridad=Alta");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var tareas = await getRes.Content.ReadFromJsonAsync<List<TareaResponse>>(
            CustomWebApplicationFactory.JsonOptions);
        Assert.NotNull(tareas);
        Assert.Contains(tareas, t => t.Titulo == tituloUnico && t.Estado == EstadoTarea.EnProgreso);
    }

    [Fact]
    public async Task PatchEstado_ConEstadoValido_Retorna200YPersisteCambio()
    {
        var proyecto = await CrearProyectoAsync();
        var postRes = await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            Titulo = "Cambiar estado HTTP",
            Estado = "Pendiente",
            Prioridad = "Baja",
            FechaLimite = DateTime.UtcNow.AddDays(2),
            ProyectoId = proyecto.Id
        });
        var tarea = await postRes.Content.ReadFromJsonAsync<TareaResponse>(
            CustomWebApplicationFactory.JsonOptions);

        var patchRes = await _client.PatchAsJsonAsync(
            $"/api/v1/tareas/{tarea!.Id}/estado",
            new { Estado = "Completada" });

        Assert.Equal(HttpStatusCode.OK, patchRes.StatusCode);

        var updated = await patchRes.Content.ReadFromJsonAsync<TareaResponse>(
            CustomWebApplicationFactory.JsonOptions);
        Assert.Equal(EstadoTarea.Completada, updated?.Estado);
    }

    [Fact]
    public async Task GetVencidas_RetornaSoloTareasVencidasNoCompletadas()
    {
        var proyecto = await CrearProyectoAsync(DateTime.UtcNow.AddDays(-10));
        var tituloVencida = $"Vencida-{Guid.NewGuid():N}";
        var tituloCompletada = $"Completada-{Guid.NewGuid():N}";

        await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            Titulo = tituloVencida,
            Estado = "Pendiente",
            Prioridad = "Alta",
            FechaLimite = DateTime.UtcNow.AddDays(-2),
            ProyectoId = proyecto.Id
        });
        await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            Titulo = tituloCompletada,
            Estado = "Completada",
            Prioridad = "Alta",
            FechaLimite = DateTime.UtcNow.AddDays(-2),
            ProyectoId = proyecto.Id
        });

        var res = await _client.GetAsync("/api/v1/tareas/vencidas");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var vencidas = await res.Content.ReadFromJsonAsync<List<TareaResponse>>(
            CustomWebApplicationFactory.JsonOptions);
        Assert.NotNull(vencidas);
        Assert.Contains(vencidas, t => t.Titulo == tituloVencida);
        Assert.DoesNotContain(vencidas, t => t.Titulo == tituloCompletada);
    }

    [Fact]
    public async Task Post_ConProyectoInexistente_Retorna404ProblemDetails()
    {
        var res = await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            Titulo = "Huérfana",
            Estado = "Pendiente",
            Prioridad = "Media",
            FechaLimite = DateTime.UtcNow.AddDays(1),
            ProyectoId = 99999
        });

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal("application/problem+json", res.Content.Headers.ContentType?.MediaType);
    }
}
