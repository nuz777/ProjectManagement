# Project Management API

API REST para la gestión de proyectos y tareas desarrollada en **.NET 10** siguiendo **Clean Architecture**, **Entity Framework Core**, **PostgreSQL** y documentación interactiva con **Swagger / OpenAPI**.

---

## Arquitectura y estructura de carpetas

```text
Projectmanage/
├── ProjectManage.slnx
├── src/
│   ├── ProjectManagement.Domain/          # Entidades (Proyecto, Tarea), enums y excepciones de dominio
│   ├── ProjectManagement.Application/     # Casos de uso / servicios, DTOs y abstracción IApplicationDbContext
│   ├── ProjectManagement.Infrastructure/  # ApplicationDbContext, configuraciones Fluent API y migraciones EF Core
│   └── ProjectManagement.Api/             # Controllers, ExceptionHandlingMiddleware, Program.cs y Swagger
└── tests/
    ├── ProjectManagement.Domain.Unit.Test/          # Pruebas unitarias de entidades y excepciones
    ├── ProjectManagement.Application.Unit.Test/     # Pruebas unitarias de servicios (EF InMemory + TimeProvider)
    ├── ProjectManagement.Infrastructure.Unit.Test/  # Pruebas de modelo, restricciones y cascada (SQLite)
    ├── ProjectManagement.Api.Unit.Test/             # Pruebas unitarias de controllers (Fakes) y middleware
    └── ProjectManagement.Api.IntegrationTests/      # Pruebas HTTP integradas (WebApplicationFactory + PostgreSQL)
```

---

## Endpoints principales

### Proyectos (`/api/v1/proyectos`)
| Método | Ruta | Descripción |
| :--- | :--- | :--- |
| `GET` | `/api/v1/proyectos` | Obtiene todos los proyectos |
| `GET` | `/api/v1/proyectos/{id}` | Obtiene un proyecto por Id |
| `POST` | `/api/v1/proyectos` | Crea un nuevo proyecto |
| `PUT` | `/api/v1/proyectos/{id}` | Actualiza un proyecto existente |
| `DELETE` | `/api/v1/proyectos/{id}` | Elimina un proyecto (si no tiene tareas activas) |
| `GET` | `/api/v1/proyectos/{id}/tareas` | Obtiene las tareas de un proyecto |
| `GET` | `/api/v1/proyectos/{id}/avance` | Calcula el porcentaje de avance del proyecto |

### Tareas (`/api/v1/tareas`)
| Método | Ruta | Descripción |
| :--- | :--- | :--- |
| `GET` | `/api/v1/tareas` | Lista tareas con filtros opcionales (`estado`, `prioridad`) |
| `GET` | `/api/v1/tareas/vencidas` | Obtiene tareas vencidas no completadas |
| `GET` | `/api/v1/tareas/{id}` | Obtiene una tarea por Id |
| `POST` | `/api/v1/tareas` | Crea una nueva tarea asociada a un proyecto |
| `PUT` | `/api/v1/tareas/{id}` | Actualiza una tarea existente |
| `DELETE` | `/api/v1/tareas/{id}` | Elimina una tarea |
| `PATCH` | `/api/v1/tareas/{id}/estado` | Cambia el estado de una tarea |

---

## Códigos HTTP de la API

Los errores de negocio y validación se transforman automáticamente a **ProblemDetails (RFC 7807, `application/problem+json`)** mediante `ExceptionHandlingMiddleware`.

| Código | Significado | Cuándo ocurre |
| :--- | :--- | :--- |
| `200 OK` | Petición exitosa | Consultas `GET`, actualizaciones `PUT` y cambio de estado `PATCH`. |
| `201 Created` | Recurso creado | `POST` de proyecto o tarea; incluye cabecera `Location`. |
| `204 No Content` | Operación sin cuerpo | `DELETE` completado correctamente. |
| `302 Found` | Redirección | `GET /` redirige hacia `/swagger`. |
| `400 Bad Request` | Datos inválidos | Validaciones de DataAnnotations o `ValidationException` (ej. `FechaFin < FechaInicio`, `FechaLimite < proyecto.FechaInicio`, enum inválido). |
| `404 Not Found` | Recurso no encontrado | `NotFoundException` cuando no existe el proyecto o la tarea solicitada. |
| `409 Conflict` | Conflicto de regla de negocio | `BusinessRuleException` al intentar eliminar un proyecto con tareas `Pendiente` o `EnProgreso`. |
| `500 Internal Server Error` | Error no controlado | Cualquier excepción inesperada capturada por el middleware. |

---

## Requisitos y ejecución local

### Requisitos
- **.NET 10 SDK** (`net10.0`)
- **PostgreSQL** ejecutándose en local (`localhost:5432`)

### Configuración de base de datos
- `src/ProjectManagement.Api/appsettings.json` mantiene `"ConnectionStrings:DefaultConnection"` vacío como configuración base para entornos productivos (sobrescribible con la variable de entorno `ConnectionStrings__DefaultConnection`).
- `src/ProjectManagement.Api/appsettings.Development.json` contiene la cadena de conexión local para el perfil `Development` definido en `Properties/launchSettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=project_management;Username=postgres;Password=TU_PASSWORD"
  }
}
```

### Migraciones y arranque
```bash
# Aplicar migraciones en PostgreSQL
dotnet ef database update --project src/ProjectManagement.Infrastructure --startup-project src/ProjectManagement.Api

# Ejecutar la API
dotnet run --project src/ProjectManagement.Api
```

Al iniciar en `Development`, la raíz `/` redirige a **`/swagger`** (con comentarios XML cargados desde `GenerateDocumentationFile`).

---

## Pruebas (Unitarias e Integradas)

El proyecto separa claramente las **pruebas unitarias** (`.Unit.Test`) de las **pruebas HTTP integradas** (`IntegrationTests`):

- **Unitarias (`.Unit.Test`):**
  - `ProjectManagement.Domain.Unit.Test`: valores por defecto de entidades y `ValidationException`.
  - `ProjectManagement.Application.Unit.Test`: lógica de `ProyectoService` y `TareaService` con EF Core InMemory y `TimeProvider` fijo.
  - `ProjectManagement.Infrastructure.Unit.Test`: mapeo de tablas, restricciones, índices y eliminación en cascada usando SQLite en memoria.
  - `ProjectManagement.Api.Unit.Test`: comportamiento de controllers con *fakes* y mapeo HTTP de `ExceptionHandlingMiddleware`.
- **Integradas (`ProjectManagement.Api.IntegrationTests`):**
  - Levanta la API completa en memoria con `WebApplicationFactory<Program>`.
  - `CustomWebApplicationFactory` reutiliza `DefaultConnection` de `appsettings.Development.json`, crea una base temporal aislada (`project_management_test_<guid>`), ejecuta `MigrateAsync()`, corre las peticiones HTTP reales con `HttpClient` y elimina la base temporal al finalizar (`EnsureDeletedAsync()`).

### Cómo ejecutarlas desde la terminal

- **Ejecutar todas las pruebas (unitarias + integradas):**
  ```bash
  dotnet test
  ```

- **Ejecutar solo las pruebas HTTP integradas** *(requiere PostgreSQL local activo en `localhost:5432`)*:
  ```bash
  dotnet test tests/ProjectManagement.Api.IntegrationTests
  ```

- **Ejecutar solo las pruebas unitarias:**
  ```bash
  dotnet test --filter "FullyQualifiedName~Unit.Test"
  ```

- **Ejecutar una capa o clase específica:**
  ```bash
  dotnet test tests/ProjectManagement.Application.Unit.Test
  dotnet test --filter "FullyQualifiedName~ProyectoServiceTests"
  ```

*(Nota: si en algún entorno .NET 10 aparece un fallo transitorio de servidores de compilación en paralelo, añade `--disable-build-servers -m:1`.)*

### Desde JetBrains Rider
- Haz clic derecho en cualquier proyecto dentro de `tests/` en el **Solution Explorer** → **Run Unit Tests**, o utiliza el botón **▶** junto a cada clase o método `[Fact]`.
