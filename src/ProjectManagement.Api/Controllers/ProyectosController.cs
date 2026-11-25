using Microsoft.AspNetCore.Mvc;
using Project_Management.Application.DTOs;
using Project_Management.Application.Services;

namespace Project_Management.Api.Controllers;

/// <summary>
/// Gestión de proyectos y consulta de sus tareas y avance.
/// </summary>
/// <param name="proyectoService">Servicio de lógica de negocio de proyectos.</param>
[ApiController]
[Route("api/v1/proyectos")]
[Produces("application/json")]
public sealed class ProyectosController(IProyectoService proyectoService) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los proyectos.
    /// </summary>
    /// <returns>Lista de proyectos.</returns>
    /// <response code="200">Devuelve la lista de proyectos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProyectoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProyectoResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await proyectoService.GetAllAsync(cancellationToken));
    }

    /// <summary>
    /// Obtiene un proyecto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del proyecto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El proyecto solicitado.</returns>
    /// <response code="200">Devuelve el proyecto.</response>
    /// <response code="404">No existe un proyecto con ese identificador.</response>
    [HttpGet("{id:int}", Name = "GetProyectoById")]
    [ProducesResponseType(typeof(ProyectoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProyectoResponse>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        return Ok(await proyectoService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Crea un nuevo proyecto.
    /// </summary>
    /// <param name="request">Datos del proyecto a crear.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El proyecto creado, con la cabecera Location correspondiente.</returns>
    /// <response code="201">Proyecto creado correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProyectoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProyectoResponse>> Create(
        [FromBody] ProyectoRequest request,
        CancellationToken cancellationToken)
    {
        var response = await proyectoService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Actualiza un proyecto existente.
    /// </summary>
    /// <param name="id">Identificador del proyecto.</param>
    /// <param name="request">Nuevos datos del proyecto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El proyecto actualizado.</returns>
    /// <response code="200">Proyecto actualizado correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No existe un proyecto con ese identificador.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProyectoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProyectoResponse>> Update(
        [FromRoute] int id,
        [FromBody] ProyectoRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await proyectoService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Elimina un proyecto si no tiene tareas pendientes o en progreso.
    /// </summary>
    /// <param name="id">Identificador del proyecto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Sin contenido.</returns>
    /// <response code="204">Proyecto eliminado correctamente.</response>
    /// <response code="404">No existe un proyecto con ese identificador.</response>
    /// <response code="409">El proyecto tiene tareas pendientes o en progreso.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await proyectoService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Obtiene todas las tareas de un proyecto.
    /// </summary>
    /// <param name="id">Identificador del proyecto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de tareas del proyecto.</returns>
    /// <response code="200">Devuelve las tareas del proyecto.</response>
    /// <response code="404">No existe un proyecto con ese identificador.</response>
    [HttpGet("{id:int}/tareas")]
    [ProducesResponseType(typeof(IEnumerable<TareaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TareaResponse>>> GetTareas(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        return Ok(await proyectoService.GetTareasAsync(id, cancellationToken));
    }

    /// <summary>
    /// Calcula el porcentaje de avance de un proyecto según sus tareas completadas.
    /// </summary>
    /// <param name="id">Identificador del proyecto.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Porcentaje de avance; 0 si el proyecto no tiene tareas.</returns>
    /// <response code="200">Devuelve el avance calculado.</response>
    /// <response code="404">No existe un proyecto con ese identificador.</response>
    [HttpGet("{id:int}/avance")]
    [ProducesResponseType(typeof(AvanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvanceResponse>> GetAvance(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        return Ok(await proyectoService.GetAvanceAsync(id, cancellationToken));
    }
}
