namespace Project_Management.Domain.Exceptions;

/// <summary>
/// Se lanza cuando los datos de entrada no satisfacen una validación de negocio.
/// </summary>
public sealed class ValidationException(
    string message,
    IDictionary<string, string[]>? errors = null) : Exception(message)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } =
        errors?.ToDictionary(pair => pair.Key, pair => pair.Value)
        ?? new Dictionary<string, string[]>();
}
