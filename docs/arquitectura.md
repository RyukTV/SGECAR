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

La autorización se expresa en **permisos**, no en nombres de rol. El rol se traduce a un conjunto de permisos (`Consultar`, `Agregar`, `Modificar`, `Eliminar`, `GestionarUsuarios`, `GestionarRoles`) y son esos permisos los que se exigen en cada punto del sistema. Así, cambiar lo que puede hacer un rol no obliga a buscar condicionales repartidos por las vistas y los controladores.

```text
JWT (claim de rol)
        ↓
CatalogoDePermisos  (Contracts, compartido)
        ↓                         ↓
Blazor: políticas            API: políticas
AuthorizeView / rutas        [Permiso(...)] + IUsuarioActual
(oculta la opción)           (rechaza la petición)
```

### Fuente de verdad compartida

`SistemaGestionEmpresarial.Contracts.Autorizacion` contiene la matriz rol → permisos (`CatalogoDePermisos`), los nombres de rol y permiso, y los nombres de política. Al vivir en Contracts, el botón que Blazor oculta y la regla que la API aplica no pueden divergir. Esto amplía ligeramente la responsabilidad de Contracts: además de los contratos HTTP, transporta las reglas de autorización que ambos lados deben interpretar igual.

Los valores del catálogo reproducen el seed de `DatabaseSeeder`, incluido el `PuedeAgregar = false` del Supervisor: la especificación recibida del PM confirma que el Supervisor consulta y modifica, el Ejecutor consulta y agrega, y solo el Administrador elimina y gestiona usuarios y roles.

### Doble barrera

Ocultar opciones en la interfaz es comodidad, no seguridad. Toda acción se verifica también en el servidor:

- **Blazor** registra una política por permiso y las usa en `AuthorizeView`, en el componente `VistaConPermiso` y en `@attribute [Authorize(Policy = ...)]` para proteger rutas.
- **La API** registra las mismas políticas y las exige con `[Permiso(...)]` en los Controllers. Además, `IUsuarioActual.ExigirPermisoAsync` permite comprobar permisos dentro de los Services cuando la regla depende de los datos y no solo del endpoint.
- La API está **cerrada por omisión** (`FallbackPolicy`): un endpoint nuevo exige sesión aunque se olvide anotarlo. Los públicos (`api/auth/login`, `api/health`) están marcados con `[AllowAnonymous]`.

### Respuestas de acceso denegado

ASP.NET Core devuelve 401 y 403 con el cuerpo vacío, que no da nada que mostrar al usuario. `RespuestaDeAutorizacionHandler` los sustituye por un `AccesoDenegadoResponse` en JSON que indica el rol y el permiso que faltaba, y `MiddlewareDePermisoDenegado` hace lo mismo con las denegaciones lanzadas desde los Services. El cliente muestra ese mensaje tal cual, sin inventarse el texto.

### Integración con los roles de SQL Server

`IProveedorDePermisos` es el único punto que debe cambiar cuando la tabla `Roles` se administre desde la aplicación: basta con una implementación que lea las columnas booleanas de la entidad `Rol` en lugar del catálogo. Los nombres de permiso ya coinciden con esas columnas, de modo que las políticas, los atributos, los Services y el cliente Blazor quedan intactos.

### Verificación

Endpoints de `api/operaciones` (`consultar`, `agregar`, `modificar`, `eliminar`), uno por permiso, para comprobar el control de acceso de extremo a extremo mientras se define el modelo de datos. La página `/operaciones` los invoca con un botón **Forzar llamada a la API** que ignora los permisos del cliente, de modo que pueda verse que es el servidor quien rechaza. Cuando existan las entidades reales, estos endpoints se sustituyen por los de negocio conservando los mismos atributos.
