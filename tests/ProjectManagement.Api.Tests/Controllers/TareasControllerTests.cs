using Microsoft.AspNetCore.Mvc;
using Project_Management.Api.Controllers;
using Project_Management.Application.DTOs;
using Project_Management.Application.Services;
using Project_Management.Domain.Enums;

namespace ProjectManagement.Api.Tests.Controllers;

public sealed class TareasControllerTests
{
    [Fact]
    public async Task GetAll_PasaFiltrosAlServicioYRetornaOk()
    {
        var service = new FakeTareaService();
        var controller = new TareasController(service);

        var actionResult = await controller.GetAll(EstadoTarea.EnProgreso, PrioridadTarea.Alta, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.IsAssignableFrom<IEnumerable<TareaResponse>>(ok.Value);
        Assert.Equal(EstadoTarea.EnProgreso, service.LastEstadoFiltro);
        Assert.Equal(PrioridadTarea.Alta, service.LastPrioridadFiltro);
    }

    [Fact]
    public async Task Create_ConRequestValido_RetornaCreatedAtAction201()
    {
        var service = new FakeTareaService();
        var controller = new TareasController(service);
        var request = new TareaRequest
        {
            Titulo = "Escribir pruebas",
            Estado = EstadoTarea.Pendiente,
            Prioridad = PrioridadTarea.Media,
            FechaLimite = DateTime.UtcNow.AddDays(1),
            ProyectoId = 1
        };

        var actionResult = await controller.Create(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(TareasController.GetById), created.ActionName);
        Assert.Equal(15, created.RouteValues?["id"]);
    }

    [Fact]
    public async Task Delete_CuandoCompleta_RetornaNoContent204()
    {
        var service = new FakeTareaService();
        var controller = new TareasController(service);

        var result = await controller.Delete(9, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(9, service.DeletedId);
    }

    [Fact]
    public async Task UpdateEstado_RetornaOkConEstadoActualizado()
    {
        var service = new FakeTareaService();
        var controller = new TareasController(service);

        var actionResult = await controller.UpdateEstado(
            5,
            new ActualizarEstadoRequest { Estado = EstadoTarea.Completada },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<TareaResponse>(ok.Value);
        Assert.Equal(5, response.Id);
        Assert.Equal(EstadoTarea.Completada, response.Estado);
    }

    private sealed class FakeTareaService : ITareaService
    {
        public EstadoTarea? LastEstadoFiltro { get; private set; }
        public PrioridadTarea? LastPrioridadFiltro { get; private set; }
        public int? DeletedId { get; private set; }

        public Task<IReadOnlyList<TareaResponse>> GetAllAsync(
            EstadoTarea? estado,
            PrioridadTarea? prioridad,
            CancellationToken cancellationToken = default)
        {
            LastEstadoFiltro = estado;
            LastPrioridadFiltro = prioridad;
            return Task.FromResult<IReadOnlyList<TareaResponse>>([]);
        }

        public Task<TareaResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(new TareaResponse(id, "Tarea", null, EstadoTarea.Pendiente, PrioridadTarea.Media, DateTime.UtcNow, 1));

        public Task<TareaResponse> CreateAsync(TareaRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new TareaResponse(15, request.Titulo, request.Descripcion, request.Estado, request.Prioridad, request.FechaLimite, request.ProyectoId));

        public Task<TareaResponse> UpdateAsync(int id, TareaRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new TareaResponse(id, request.Titulo, request.Descripcion, request.Estado, request.Prioridad, request.FechaLimite, request.ProyectoId));

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            DeletedId = id;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TareaResponse>> GetVencidasAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TareaResponse>>([]);

        public Task<TareaResponse> UpdateEstadoAsync(int id, ActualizarEstadoRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new TareaResponse(id, "Tarea", null, request.Estado, PrioridadTarea.Media, DateTime.UtcNow, 1));
    }
}
