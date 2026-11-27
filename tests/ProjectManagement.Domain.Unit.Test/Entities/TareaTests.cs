using Project_Management.Domain.Entities;
using Project_Management.Domain.Enums;

namespace ProjectManagement.Domain.Unit.Test.Entities;

public sealed class TareaTests
{
    [Fact]
    public void Constructor_AlInstanciar_InicializaPropiedadesPorDefecto()
    {
        var tarea = new Tarea();

        Assert.Equal(string.Empty, tarea.Titulo);
        Assert.Null(tarea.Descripcion);
        Assert.Equal(EstadoTarea.Pendiente, tarea.Estado);
        Assert.Equal(PrioridadTarea.Baja, tarea.Prioridad);
    }
}
