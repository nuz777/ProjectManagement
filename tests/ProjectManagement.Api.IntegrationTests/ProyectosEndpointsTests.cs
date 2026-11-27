using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Project_Management.Application.DTOs;

namespace ProjectManagement.Api.IntegrationTests;

public sealed class ProyectosEndpointsTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostYGetById_ConProyectoValido_Retorna201Y200()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/v1/proyectos", new
        {
            Nombre = "Proyecto Integración",
            Descripcion = "Flujo completo HTTP",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(10)
        });

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        var creado = await postResponse.Content.ReadFromJsonAsync<ProyectoResponse>(
            CustomWebApplicationFactory.JsonOptions);
        Assert.NotNull(creado);
        Assert.Equal("Proyecto Integración", creado.Nombre);

        var getResponse = await _client.GetAsync($"/api/v1/proyectos/{creado.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var obtenido = await getResponse.Content.ReadFromJsonAsync<ProyectoResponse>(
            CustomWebApplicationFactory.JsonOptions);
        Assert.Equal(creado.Id, obtenido?.Id);
    }

    [Fact]
    public async Task Post_CuandoFechaFinEsMenorAInicio_Retorna400ProblemDetails()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/proyectos", new
        {
            Nombre = "Fechas inválidas",
            FechaInicio = DateTime.UtcNow.AddDays(5),
            FechaFin = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.True(doc.RootElement.GetProperty("errors").TryGetProperty("FechaFin", out _));
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_Retorna404ProblemDetails()
    {
        var response = await _client.GetAsync("/api/v1/proyectos/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Delete_CuandoTieneTareaActiva_Retorna409Conflict()
    {
        var proyectoRes = await _client.PostAsJsonAsync("/api/v1/proyectos", new
        {
            Nombre = "Proyecto Con Tarea Activa",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(5)
        });
        proyectoRes.EnsureSuccessStatusCode();
        var proyecto = await proyectoRes.Content.ReadFromJsonAsync<ProyectoResponse>(
            CustomWebApplicationFactory.JsonOptions);

        var tareaRes = await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            Titulo = "Tarea bloqueante",
            Estado = "Pendiente",
            Prioridad = "Alta",
            FechaLimite = DateTime.UtcNow.AddDays(2),
            ProyectoId = proyecto!.Id
        });
        tareaRes.EnsureSuccessStatusCode();

        var deleteRes = await _client.DeleteAsync($"/api/v1/proyectos/{proyecto.Id}");

        Assert.Equal(HttpStatusCode.Conflict, deleteRes.StatusCode);
    }

    [Fact]
    public async Task GetAvance_ConTareasMixtas_Retorna200ConPorcentajeCalculado()
    {
        var proyectoRes = await _client.PostAsJsonAsync("/api/v1/proyectos", new
        {
            Nombre = "Proyecto Avance",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(5)
        });
        var proyecto = await proyectoRes.Content.ReadFromJsonAsync<ProyectoResponse>(
            CustomWebApplicationFactory.JsonOptions);

        await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            Titulo = "Hecha",
            Estado = "Completada",
            Prioridad = "Media",
            FechaLimite = DateTime.UtcNow.AddDays(1),
            ProyectoId = proyecto!.Id
        });
        await _client.PostAsJsonAsync("/api/v1/tareas", new
        {
            Titulo = "Pendiente",
            Estado = "Pendiente",
            Prioridad = "Media",
            FechaLimite = DateTime.UtcNow.AddDays(2),
            ProyectoId = proyecto.Id
        });

        var avanceRes = await _client.GetAsync($"/api/v1/proyectos/{proyecto.Id}/avance");

        Assert.Equal(HttpStatusCode.OK, avanceRes.StatusCode);
        var avance = await avanceRes.Content.ReadFromJsonAsync<AvanceResponse>(
            CustomWebApplicationFactory.JsonOptions);
        Assert.NotNull(avance);
        Assert.Equal(2, avance.TotalTareas);
        Assert.Equal(1, avance.TareasCompletadas);
        Assert.Equal(50, avance.Porcentaje);
    }
}
