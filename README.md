# SGECAR

Sistema de Gestión Empresarial con Control de Acceso por Roles, desarrollado como proyecto académico por un equipo de tres integrantes.

## Tecnologías

- C# y .NET 8
- Blazor WebAssembly independiente
- ASP.NET Core Web API con Controllers
- Entity Framework Core 8
- SQL Server Express
- JWT y autorización basada en permisos

## Arquitectura

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

La solución contiene tres proyectos:

- `SistemaGestionEmpresarial.Web`: interfaz Blazor WebAssembly.
- `SistemaGestionEmpresarial.Api`: API, autenticación, servicios y persistencia.
- `SistemaGestionEmpresarial.Contracts`: contratos HTTP compartidos, sin lógica de negocio.

La explicación ampliada está en [docs/arquitectura.md](docs/arquitectura.md).

## Funcionalidades de la Etapa I

- Inicio y cierre de sesión con JWT.
- Sesión persistente en el navegador.
- Bloqueo durante un minuto después de tres intentos fallidos.
- Autorización por permisos en Blazor y en la API.
- CRUD y búsqueda de usuarios.
- CRUD y búsqueda de roles y sus permisos.
- Validación de nombres duplicados y relaciones existentes.
- Contraseñas almacenadas exclusivamente como hash.
- Migración y datos iniciales idempotentes en Development.

## Roles iniciales

| Rol | Consultar | Agregar | Modificar | Eliminar | Gestionar usuarios | Gestionar roles |
| --- | --- | --- | --- | --- | --- | --- |
| Administrador | Sí | Sí | Sí | Sí | Sí | Sí |
| Supervisor | Sí | No | Sí | No | No | No |
| Ejecutor | Sí | Sí | No | No | No | No |

La especificación académica no definía si Supervisor podía agregar. Para la Etapa I se tomó explícitamente la decisión de usar `PuedeAgregar = false`; el equipo puede revisarla con el profesor sin cambiar el modelo de autorización.

## Requisitos y ejecución local

Se requiere .NET SDK 8, SQL Server Express en `localhost\SQLEXPRESS` y la base `SistemaGestionEmpresarialDb`, que se crea y migra automáticamente al iniciar la API en Development.

```bash
dotnet restore
dotnet tool restore
dotnet user-secrets set "Jwt:Key" "REEMPLAZAR-POR-UNA-CLAVE-LARGA-Y-ALEATORIA" --project src/SistemaGestionEmpresarial.Api
```

La clave JWT es obligatoria y no debe guardarse en archivos versionados. Para iniciar manualmente, usar dos terminales:

```bash
dotnet run --project src/SistemaGestionEmpresarial.Api --launch-profile http
dotnet run --project src/SistemaGestionEmpresarial.Web --launch-profile http
```

También puede ejecutarse `iniciar.bat`, que abre ambos proyectos y el navegador. `detener.bat` detiene los procesos iniciados.

- API: `http://localhost:5080`
- Web: `http://localhost:5180`
- Swagger en Development: `http://localhost:5080/swagger`

Usuarios de desarrollo creados por el seeder:

| Usuario | Contraseña inicial | Rol |
| --- | --- | --- |
| `admin` | `admin123` | Administrador |
| `supervisor` | `supervisor123` | Supervisor |
| `ejecutor` | `ejecutor123` | Ejecutor |

Estas credenciales son únicamente para desarrollo académico y deben cambiarse o eliminarse antes de cualquier despliegue real.

## Seguridad

- La API está cerrada por omisión; solo login y health son anónimos.
- Los permisos proceden de SQL Server, se incluyen como claims en el JWT y se vuelven a cargar en el siguiente inicio de sesión.
- Ocultar controles en Blazor no sustituye la validación: cada endpoint administrativo exige el permiso correspondiente.
- Los contratos nunca exponen `PasswordHash`.
- La clave JWT se configura con User Secrets, no en `appsettings.json`.

Los resultados y capturas de la validación integral están en [docs/ETAPA-I.md](docs/ETAPA-I.md) y [docs/evidencias/etapa-1/RESULTADOS_PRUEBAS.md](docs/evidencias/etapa-1/RESULTADOS_PRUEBAS.md).

## Flujo de trabajo Git

```text
main
  ↑ Pull Request revisado
rama de trabajo
```

`main` representa la versión estable. No se desarrolla, hace push, merge ni force push directamente sobre ella. Todo cambio debe realizarse en una rama y entrar mediante Pull Request después de compilarse, probarse y revisarse.

Tipos de ramas recomendados:

- `feature/<descripcion>`: funcionalidad nueva.
- `fix/<descripcion>`: corrección.
- `docs/<descripcion>`: documentación.
- `refactor/<descripcion>`: reorganización sin cambio funcional.
- `test/<descripcion>`: pruebas.
- `chore/<descripcion>`: configuración o mantenimiento.

Crear una rama desde `main` actualizado:

```bash
git switch main
git pull origin main
git switch -c feature/nombre
```

Usar commits breves y claros, por ejemplo `feat: agrega login` o `fix: corrige validacion de usuario`. Después:

```bash
git push -u origin feature/nombre
```

Si hay conflictos importantes o dos integrantes deben modificar la misma funcionalidad, deben coordinarse y revisar el código afectado antes de resolverlos.
