using Project_Management.Domain.Exceptions;

namespace ProjectManagement.Domain.Tests.Exceptions;

public sealed class ValidationExceptionTests
{
    [Fact]
    public void Constructor_SinDiccionarioDeErrores_InicializaErrorsVacio()
    {
        var exception = new ValidationException("Datos inválidos.");

        Assert.Equal("Datos inválidos.", exception.Message);
        Assert.Empty(exception.Errors);
    }

    [Fact]
    public void Constructor_ConErrores_ConservaDetalleDeErrores()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["FechaFin"] = ["Debe ser posterior o igual a FechaInicio."]
        };

        var exception = new ValidationException("Error de validación.", errors);

        Assert.Single(exception.Errors);
        Assert.Equal(["Debe ser posterior o igual a FechaInicio."], exception.Errors["FechaFin"]);
    }
}
