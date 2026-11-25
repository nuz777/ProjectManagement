using Microsoft.AspNetCore.Mvc;
using Project_Management.Application.DTOs;
using Project_Management.Domain.Enums;
using Project_Management.Application.Services;

namespace Project_Management.Api.Controllers;

/// <summary>
/// Gestión y consulta de tareas.
/// </summary>
/// <param name="tareaService">Servicio de lógica de negocio de tareas.</param>
[ApiController]
[Route("api/v1/tareas")]
[Produces("application/json")]
public sealed class TareasController(ITareaService tareaService) : ControllerBase
{
  
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TareaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TareaResponse>>> GetAll(
        [FromQuery] EstadoTarea? estado,
        [FromQuery] PrioridadTarea? prioridad,
        CancellationToken cancellationToken)
    {
        return Ok(await tareaService.GetAllAsync(estado, prioridad, cancellationToken));
    }

    /// <summary>
    /// Obtiene las tareas vencidas: con fecha límite pasada y estado distinto de Completada.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de tareas vencidas.</returns>
    /// <response code="200">Devuelve la lista de tareas vencidas.</response>
    [HttpGet("vencidas")]
    [ProducesResponseType(typeof(IEnumerable<TareaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TareaResponse>>> GetVencidas(CancellationToken cancellationToken)
    {
        return Ok(await tareaService.GetVencidasAsync(cancellationToken));
    }

    /// <summary>
    /// Obtiene una tarea por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la tarea.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La tarea solicitada.</returns>
    /// <response code="200">Devuelve la tarea.</response>
    /// <response code="404">No existe una tarea con ese identificador.</response>
    [HttpGet("{id:int}", Name = "GetTareaById")]
    [ProducesResponseType(typeof(TareaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaResponse>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        return Ok(await tareaService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Crea una nueva tarea asociada a un proyecto existente.
    /// </summary>
    /// <param name="request">Datos de la tarea a crear.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La tarea creada, con la cabecera Location correspondiente.</returns>
    /// <response code="201">Tarea creada correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No existe el proyecto indicado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TareaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaResponse>> Create(
        [FromBody] TareaRequest request,
        CancellationToken cancellationToken)
    {
        var response = await tareaService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Actualiza una tarea existente.
    /// </summary>
    /// <param name="id">Identificador de la tarea.</param>
    /// <param name="request">Nuevos datos de la tarea.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La tarea actualizada.</returns>
    /// <response code="200">Tarea actualizada correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No existe la tarea o el proyecto indicado.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TareaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaResponse>> Update(
        [FromRoute] int id,
        [FromBody] TareaRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await tareaService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Elimina una tarea por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la tarea.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Sin contenido.</returns>
    /// <response code="204">Tarea eliminada correctamente.</response>
    /// <response code="404">No existe una tarea con ese identificador.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await tareaService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Cambia el estado de una tarea.
    /// </summary>
    /// <param name="id">Identificador de la tarea.</param>
    /// <param name="request">Nuevo estado de la tarea.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La tarea con su estado actualizado.</returns>
    /// <response code="200">Estado actualizado correctamente.</response>
    /// <response code="400">El estado indicado no es válido.</response>
    /// <response code="404">No existe una tarea con ese identificador.</response>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(TareaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaResponse>> UpdateEstado(
        [FromRoute] int id,
        [FromBody] ActualizarEstadoRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await tareaService.UpdateEstadoAsync(id, request, cancellationToken));
    }
}
