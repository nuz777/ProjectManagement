using Microsoft.AspNetCore.Mvc;
using Project_Management.Api.Controllers;
using Project_Management.Application.DTOs;
using Project_Management.Application.Services;

namespace ProjectManagement.Api.Tests.Controllers;

public sealed class ProyectosControllerTests
{
    [Fact]
    public async Task GetAll_RetornaOkConListaDeProyectos()
    {
        var service = new FakeProyectoService();
        var controller = new ProyectosController(service);

        var actionResult = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<ProyectoResponse>>(ok.Value);
        Assert.Single(items);
    }

    [Fact]
    public async Task Create_ConRequestValido_RetornaCreatedAtAction201()
    {
        var service = new FakeProyectoService();
        var controller = new ProyectosController(service);
        var request = new ProyectoRequest
        {
            Nombre = "Nuevo",
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(5)
        };

        var actionResult = await controller.Create(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(ProyectosController.GetById), created.ActionName);
        Assert.Equal(42, created.RouteValues?["id"]);
        var payload = Assert.IsType<ProyectoResponse>(created.Value);
        Assert.Equal("Nuevo", payload.Nombre);
    }

    [Fact]
    public async Task Delete_CuandoCompleta_RetornaNoContent204()
    {
        var service = new FakeProyectoService();
        var controller = new ProyectosController(service);

        var result = await controller.Delete(7, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(7, service.DeletedId);
    }

    [Fact]
    public async Task GetAvance_RetornaOkConAvance()
    {
        var service = new FakeProyectoService();
        var controller = new ProyectosController(service);

        var actionResult = await controller.GetAvance(3, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var avance = Assert.IsType<AvanceResponse>(ok.Value);
        Assert.Equal(3, avance.ProyectoId);
        Assert.Equal(50, avance.Porcentaje);
    }

    private sealed class FakeProyectoService : IProyectoService
    {
        public int? DeletedId { get; private set; }

        public Task<IReadOnlyList<ProyectoResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ProyectoResponse>>(
            [
                new ProyectoResponse(1, "Demo", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1))
            ]);

        public Task<ProyectoResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ProyectoResponse(id, "Demo", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));

        public Task<ProyectoResponse> CreateAsync(ProyectoRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ProyectoResponse(42, request.Nombre, request.Descripcion, request.FechaInicio, request.FechaFin));

        public Task<ProyectoResponse> UpdateAsync(int id, ProyectoRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ProyectoResponse(id, request.Nombre, request.Descripcion, request.FechaInicio, request.FechaFin));

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            DeletedId = id;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TareaResponse>> GetTareasAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TareaResponse>>([]);

        public Task<AvanceResponse> GetAvanceAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AvanceResponse(id, 2, 1, 50));
    }
}
