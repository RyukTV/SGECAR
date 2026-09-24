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

La solución mantiene una arquitectura deliberadamente sencilla. No se añadieron proyectos Domain, Application o Infrastructure, CQRS, repositorios genéricos ni otras capas que no necesita el alcance académico actual.

## Responsabilidades

- `SistemaGestionEmpresarial.Web` ejecuta la interfaz en el navegador, conserva el JWT y llama a la API por HTTP.
- `SistemaGestionEmpresarial.Api` contiene Controllers, Services, autorización, `AppDbContext`, entidades y migraciones.
- `SistemaGestionEmpresarial.Contracts` comparte exclusivamente contratos HTTP entre cliente y servidor. Web no referencia Api.
- SQL Server es la fuente persistente de usuarios, roles, permisos, intentos fallidos y bloqueos.

Los Controllers traducen HTTP a operaciones de aplicación. Los Services contienen las validaciones y usan `AppDbContext` directamente; no se agregó Repository Pattern ni Unit of Work personalizado porque EF Core ya cubre esas responsabilidades.

## Flujo de autenticación

```text
Login.razor
    ↓ POST /api/auth/login
AuthController
    ↓
AuthService
    ↓ consulta Usuario + Rol en SQL Server
PasswordHasher verifica la contraseña
    ↓
JWT: identidad + rol + claims de permiso
    ↓
Blazor guarda la sesión y configura Authorization
```

Tres credenciales inválidas incrementan `IntentosFallidos` y establecen `BloqueadoHasta` durante un minuto. El bloqueo reside en SQL Server, por lo que persiste al recargar el navegador o reiniciar la API. Un acceso correcto después de expirar el bloqueo restablece ambos campos.

## Autorización por permisos

Los permisos son `Consultar`, `Agregar`, `Modificar`, `Eliminar`, `GestionarUsuarios` y `GestionarRoles`. Su fuente de verdad son las columnas booleanas de `Roles`.

`AuthService` transforma esas columnas en claims `permiso` del JWT. No existe una matriz de nombres de rol hardcodeada para autorizar. Un rol nuevo funciona con los permisos seleccionados y los cambios se reflejan cuando el usuario inicia sesión nuevamente y recibe un JWT nuevo.

```text
SQL Server / Roles
        ↓
AuthService
        ↓
JWT con claims "permiso"
        ↓                         ↓
Blazor: políticas            API: políticas
menús y rutas                [Permiso(...)]
```

Blazor muestra u oculta elementos según las políticas, pero esa medida es solo de experiencia de usuario. La API constituye la barrera de seguridad y vuelve a validar cada petición. Está cerrada por omisión mediante `FallbackPolicy`; `api/auth/login` y `api/health` son las excepciones anónimas. Las respuestas 401 y 403 usan un contrato JSON comprensible.

## Administración de usuarios y roles

`UsuariosController` delega en `UsuariosService` el listado, búsqueda, consulta, creación, modificación y eliminación de usuarios. Todo el Controller exige `GestionarUsuarios`. El Service valida nombres únicos, roles existentes y contraseñas; utiliza `PasswordHasher<Usuario>` y nunca devuelve `PasswordHash`.

`RolesController` delega en `RolesService` el CRUD, búsqueda y actualización de permisos. Todo el Controller exige `GestionarRoles`. No permite eliminar un rol mientras tenga usuarios asignados.

En Web, `UsuariosApiService` y `RolesApiService` centralizan las llamadas HTTP de las páginas `/usuarios` y `/roles`.

## Persistencia

`AppDbContext` administra `Usuarios` y `Roles`. En Development, la API ejecuta `Database.MigrateAsync()` y después un seeder idempotente. La migración `InitialUsersAndRoles` crea el esquema; [database/SGECAR.sql](../database/SGECAR.sql) corresponde al mismo modelo y no contiene datos iniciales.

El seeder crea tres roles y tres usuarios solo cuando faltan. Las contraseñas de desarrollo se procesan con `PasswordHasher<Usuario>` y no se almacenan en texto plano.

La decisión provisional documentada para Supervisor es `PuedeAgregar = false`, porque la consigna original no definía ese permiso.

## Configuración local

- SQL Server: `localhost\SQLEXPRESS`
- Base: `SistemaGestionEmpresarialDb`
- API: `http://localhost:5080`
- Blazor: `http://localhost:5180`
- CORS: permite únicamente `http://localhost:5180` en la política de desarrollo.
- `Jwt:Key`: User Secrets; nunca se versiona.

La Etapa I no modificó el esquema después de `InitialUsersAndRoles`, por lo que no requirió una migración adicional.
