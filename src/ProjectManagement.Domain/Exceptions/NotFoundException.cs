namespace Project_Management.Domain.Exceptions;

/// <summary>
/// Se lanza cuando no existe el recurso solicitado.
/// </summary>
public sealed class NotFoundException(string message) : Exception(message);
