using Project_Management.Application.DTOs;

namespace Project_Management.Application.Services;

public interface IProyectoService
{
    Task<IReadOnlyList<ProyectoResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProyectoResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProyectoResponse> CreateAsync(ProyectoRequest request, CancellationToken cancellationToken = default);
    Task<ProyectoResponse> UpdateAsync(int id, ProyectoRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TareaResponse>> GetTareasAsync(int id, CancellationToken cancellationToken = default);
    Task<AvanceResponse> GetAvanceAsync(int id, CancellationToken cancellationToken = default);
}
