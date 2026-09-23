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

## Control de acceso por roles

La autorización se expresa en **permisos**, no en nombres de rol repartidos por el código. Los permisos disponibles son `Consultar`, `Agregar`, `Modificar`, `Eliminar`, `GestionarUsuarios` y `GestionarRoles`.

La fuente de verdad es la tabla `Roles` de SQL Server. Al iniciar sesión, `AuthService` ya tiene cargado el `Rol` del usuario y convierte sus columnas booleanas (`PuedeConsultar`, `PuedeAgregar`, etc.) en claims `permiso` dentro del JWT.

```text
SQL Server / Roles
        ↓
AuthService
        ↓
JWT: rol + claims "permiso"
        ↓                         ↓
Blazor: políticas            API: políticas
AuthorizeView / rutas        [Permiso(...)] + IUsuarioActual
(oculta la opción)           (rechaza la petición)
```

### Fuente de verdad

No existe una segunda matriz rol → permisos hardcodeada para decidir la autorización. Blazor y la API leen los mismos claims del JWT, que fueron generados a partir del registro de `Rol` en SQL Server.

Esto permite que un rol nuevo funcione sin agregar su nombre al código: basta con que exista en la base de datos, tenga configuradas sus columnas de permisos y un usuario inicie sesión con ese rol.

Si los permisos de un rol cambian mientras un usuario ya tiene una sesión activa, el cambio se refleja en un nuevo JWT al volver a iniciar sesión.

### Doble barrera

Ocultar opciones en la interfaz es comodidad, no seguridad. Toda acción se verifica también en el servidor:

- **Blazor** registra una política por permiso y las usa en `AuthorizeView`, en el componente `VistaConPermiso` y en `[Authorize(Policy = ...)]` para proteger rutas.
- **La API** registra las mismas políticas y las exige con `[Permiso(...)]` en los Controllers. `IUsuarioActual.ExigirPermisoAsync` permite repetir la comprobación dentro de Services cuando sea necesario.
- La API está **cerrada por omisión** mediante `FallbackPolicy`. Los endpoints públicos `api/auth/login` y `api/health` están marcados con `[AllowAnonymous]`.

### Respuestas de acceso denegado

`RespuestaDeAutorizacionHandler` transforma los 401 y 403 en un `AccesoDenegadoResponse` JSON que el cliente puede mostrar. `MiddlewareDePermisoDenegado` aplica el mismo formato a las denegaciones lanzadas desde Services.

### Catálogo administrativo

`GET api/permisos/catalogo` consulta directamente `AppDbContext.Roles` y construye la matriz visible a partir de las columnas booleanas de cada rol. El endpoint requiere `GestionarRoles`.

### Verificación de la Etapa I

Los endpoints de `api/operaciones` (`consultar`, `agregar`, `modificar`, `eliminar`) sirven para demostrar el control de acceso extremo a extremo. La página `/operaciones` permite incluso forzar una petición para demostrar que el backend rechaza una acción aunque el cliente intente ejecutarla.

Estos endpoints son de demostración de permisos; no sustituyen los CRUD de negocio que se implementen en etapas posteriores.
