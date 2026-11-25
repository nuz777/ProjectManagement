namespace Project_Management.Domain.Exceptions;

/// <summary>
/// Se lanza cuando una operación vulnera una regla de negocio.
/// </summary>
public sealed class BusinessRuleException(string message) : Exception(message);
