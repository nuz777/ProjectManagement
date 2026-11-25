using Project_Management.Application.DTOs;
using Project_Management.Domain.Enums;

namespace Project_Management.Application.Services;

public interface ITareaService
{
    Task<IReadOnlyList<TareaResponse>> GetAllAsync(
        EstadoTarea? estado,
        PrioridadTarea? prioridad,
        CancellationToken cancellationToken = default);

    Task<TareaResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TareaResponse> CreateAsync(TareaRequest request, CancellationToken cancellationToken = default);
    Task<TareaResponse> UpdateAsync(int id, TareaRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TareaResponse>> GetVencidasAsync(CancellationToken cancellationToken = default);
    Task<TareaResponse> UpdateEstadoAsync(
        int id,
        ActualizarEstadoRequest request,
        CancellationToken cancellationToken = default);
}
