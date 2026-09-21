# Arquitectura técnica

```text
Blazor WebAssembly
        ↓ HTTP/JSON
ASP.NET Core Web API
        ↓
Controllers
        ↓
Services
        ↓
Entity Framework Core
        ↓
SQL Server
```

`SistemaGestionEmpresarial.Contracts` se utiliza exclusivamente para compartir contratos HTTP entre el cliente Blazor y la API. El cliente no referencia el proyecto de la API: se comunica con él únicamente por HTTP.

Por ahora se descartó una arquitectura completa con proyectos `Domain`, `Application` e `Infrastructure`, porque no es necesaria para el alcance actual. Los servicios y las entidades se agregarán cuando el equipo defina las funcionalidades y el modelo de datos.

## Persistencia de Etapa I

La API utiliza `AppDbContext` para `Roles` y `Usuarios`. En Development aplica las migraciones pendientes y ejecuta un seed idempotente de tres roles y tres usuarios con contraseñas de ejemplo protegidas mediante `PasswordHasher<Usuario>`. Este seed no se ejecuta fuera de Development; las credenciales de ejemplo no deben utilizarse en producción. `database/SGECAR.sql` contiene el esquema generado por EF, mientras que los datos iniciales provienen del seeder.

La práctica no especifica si el rol Supervisor puede agregar. Se configuró `PuedeAgregar = false` provisionalmente; el equipo debe validar esta decisión con el profesor antes de considerar definitivos los permisos.
