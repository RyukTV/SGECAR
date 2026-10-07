# Cierre técnico de la Etapa I

## Objetivo

Entregar un sistema funcional de autenticación y control de acceso por roles sobre el flujo Blazor WebAssembly → HTTP/JSON → Web API → Services → Entity Framework Core → SQL Server.

## Implementado

- Login y cierre de sesión.
- Persistencia de sesión en el navegador.
- Usuarios, roles y permisos procedentes de SQL Server.
- CRUD y búsqueda de usuarios y roles.
- Validaciones de datos, duplicados y relaciones.
- Navegación y autorización en frontend y backend.
- Bloqueo persistente por intentos fallidos.

## Flujo de Login

Blazor envía las credenciales a `AuthController`; `AuthService` consulta el usuario y su rol, verifica la contraseña con `PasswordHasher<Usuario>` y, si son válidos, emite un JWT con identidad, rol y permisos. Blazor conserva el token para la sesión y lo adjunta a las peticiones siguientes.

## Matriz de permisos

| Rol | Consultar | Agregar | Modificar | Eliminar | Gestionar usuarios | Gestionar roles |
| --- | --- | --- | --- | --- | --- | --- |
| Administrador | Sí | Sí | Sí | Sí | Sí | Sí |
| Supervisor | Sí | No | Sí | No | No | No |
| Ejecutor | Sí | Sí | No | No | No | No |

Supervisor conserva `PuedeAgregar = false` como decisión explícita de esta etapa.

## Seguridad

- Las contraseñas se almacenan como hash, nunca en texto plano.
- El JWT transporta los claims de rol y permisos.
- Tres fallos provocan un bloqueo persistente de un minuto.
- La API responde 401 cuando falta autenticación y 403 cuando falta permiso.
- La interfaz adapta la navegación, pero los permisos siempre vuelven a comprobarse en el backend.
- El usuario autenticado no puede eliminarse ni desactivarse a sí mismo; ambas reglas se validan en la API mediante su identificador del JWT.

## Base de datos

Las tablas principales son `Usuarios` y `Roles`. La relación `Usuarios.RolId` determina el rol y sus seis permisos booleanos. La migración vigente es `InitialUsersAndRoles`; no fue necesario modificar el esquema para completar el CRUD.

## Evidencias

Se verificaron los tres inicios de sesión, bloqueo, 401, 403, matriz de permisos, CRUD, búsquedas, duplicados, `PasswordHash`, roles personalizados y actualización de permisos tras un nuevo login.

El informe detallado y las 19 capturas reales están en [evidencias/etapa-1/RESULTADOS_PRUEBAS.md](evidencias/etapa-1/RESULTADOS_PRUEBAS.md).
