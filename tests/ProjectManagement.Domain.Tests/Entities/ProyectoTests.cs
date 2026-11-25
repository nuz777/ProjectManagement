using Project_Management.Domain.Entities;

namespace ProjectManagement.Domain.Tests.Entities;

public sealed class ProyectoTests
{
    [Fact]
    public void Constructor_AlInstanciar_InicializaPropiedadesPorDefecto()
    {
        var proyecto = new Proyecto();

        Assert.Equal(string.Empty, proyecto.Nombre);
        Assert.Null(proyecto.Descripcion);
        Assert.NotNull(proyecto.Tareas);
        Assert.Empty(proyecto.Tareas);
    }
}
