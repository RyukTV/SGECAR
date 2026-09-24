# Resultados de pruebas — Etapa I

- Fecha: 2026-09-23
- Rama: `feature/admin-usuarios-roles`
- Código funcional probado: `aa04d0a`
- Entorno: .NET SDK 8.0.425, Blazor WebAssembly, ASP.NET Core Web API y SQL Server Express `localhost\SQLEXPRESS`

Las comprobaciones se ejecutaron contra la aplicación y `SistemaGestionEmpresarialDb` reales. Los registros temporales fueron eliminados; la base quedó nuevamente con los tres roles y tres usuarios iniciales.

| Prueba | Resultado esperado | Resultado obtenido | Estado | Evidencia |
| --- | --- | --- | --- | --- |
| Login Admin | Autenticar y emitir JWT | HTTP 200; rol y permisos correctos | PASS | [01](01-login.png), [03](03-admin-inicio.png) |
| Login Supervisor | Autenticar y emitir JWT | HTTP 200; rol Supervisor | PASS | [11](11-supervisor.png) |
| Login Ejecutor | Autenticar y emitir JWT | HTTP 200; rol Ejecutor | PASS | [12](12-ejecutor.png) |
| Campos vacíos | Rechazar usuario y contraseña vacíos | Ambos casos devolvieron HTTP 400 y mensaje entendible | PASS | [02](02-login-validacion.png) |
| Contraseña incorrecta | Rechazar e indicar intentos restantes | Fallos 1 y 2 rechazados con contador restante | PASS | — |
| Bloqueo por intentos | Bloquear en el tercer fallo | Tercer fallo estableció bloqueo de un minuto | PASS | [16](16-bloqueo-login.png) |
| Persistencia del bloqueo | Seguir bloqueado tras recarga y con clave correcta | Recarga y clave correcta fueron rechazadas durante el bloqueo | PASS | [16](16-bloqueo-login.png) |
| Expiración del bloqueo | Permitir acceso y limpiar campos | Después de 65 s autenticó; `IntentosFallidos=0` y `BloqueadoHasta=NULL` | PASS | — |
| 401 sin token | Rechazar endpoint protegido | `GET /api/usuarios` devolvió HTTP 401 y JSON comprensible | PASS | [14](14-api-401.png) |
| 403 por permiso | Rechazar token sin permiso | Supervisor contra `/api/usuarios` devolvió HTTP 403 | PASS | [15](15-api-403.png) |
| Matriz Administrador | Autorizar las seis capacidades | Las seis peticiones devolvieron HTTP 200 | PASS | [03](03-admin-inicio.png) |
| Matriz Supervisor | Solo Consultar y Modificar | 200 en ambas; 403 en Agregar, Eliminar, Usuarios y Roles | PASS | [11](11-supervisor.png) |
| Matriz Ejecutor | Solo Consultar y Agregar | 200 en ambas; 403 en Modificar, Eliminar, Usuarios y Roles | PASS | [12](12-ejecutor.png), [13](13-acceso-denegado.png) |
| Crear Usuario | Guardar usuario válido con rol | Creado desde Blazor y visible en listado | PASS | [05](05-usuarios-crear.png) |
| Consultar Usuario | Listar y obtener por id sin hash | Listado y detalle HTTP 200; DTO sin `PasswordHash` | PASS | [04](04-usuarios-listado.png) |
| Modificar Usuario | Editar nombre, rol y estado | Cambios persistidos; bloqueo no fue alterado | PASS | [06](06-usuarios-editar.png) |
| Contraseña opcional | Conservar hash vacío; renovarlo con valor | Hash conservado sin clave y cambiado con nueva clave | PASS | — |
| Eliminar Usuario | Eliminar registro existente | HTTP 200 y registro ausente | PASS | — |
| Búsqueda Usuario | Buscar por usuario, nombre y rol | Las tres modalidades devolvieron el registro correcto | PASS | [07](07-usuarios-busqueda.png) |
| Usuario duplicado | Rechazar nombre repetido | HTTP 409 | PASS | — |
| Rol inexistente | Rechazar `RolId` inválido | HTTP 400 | PASS | — |
| Crear Rol | Guardar nombre y seis booleanos | `PruebaEtapa1` creado con valores seleccionados | PASS | [09](09-roles-crear.png) |
| Consultar Rol | Listar y obtener por id | Ambas operaciones devolvieron HTTP 200 | PASS | [08](08-roles-listado.png) |
| Modificar Rol | Persistir permisos nuevos | Agregar cambió a false y Modificar a true | PASS | [10](10-roles-permisos.png) |
| Eliminar Rol | Eliminar rol sin usuarios | HTTP 200 y registro ausente | PASS | — |
| Búsqueda Rol | Filtrar por nombre | Se obtuvo únicamente el rol buscado | PASS | [08](08-roles-listado.png) |
| Rol con usuarios | Impedir eliminación | HTTP 409 y mensaje entendible | PASS | — |
| Rol duplicado | Rechazar nombre repetido | HTTP 409 | PASS | — |
| `PasswordHash` no expuesto | Omitirlo de toda respuesta | No aparece en contratos ni respuestas de lista/detalle | PASS | — |
| Hash en SQL Server | No almacenar claves en texto plano | Existe hash; no contiene claves iniciales ni temporales | PASS | [19](19-sql-usuarios.png) |
| Rol nuevo funciona | Autorizar por booleanos, no por nombre | Usuario temporal pudo Consultar/Agregar y recibió 403 en lo demás | PASS | [17](17-rol-personalizado.png), [18](18-sql-roles.png) |
| Cambio de permisos | Aplicar cambios en un nuevo JWT | Tras nuevo login pudo Consultar/Modificar y dejó de poder Agregar | PASS | [10](10-roles-permisos.png), [17](17-rol-personalizado.png) |
| `dotnet restore` | Restaurar solución | Proyectos actualizados correctamente | PASS | Salida de consola del cierre |
| `dotnet tool restore` | Restaurar EF 8 | `dotnet-ef` 8.0.31 restaurado | PASS | Salida de consola del cierre |
| `dotnet build --no-restore` | 0 errores y 0 warnings | Compilación correcta: 0 errores, 0 advertencias | PASS | Salida de consola del cierre |

## Seguridad comprobada

- Los campos de contraseña usan `type="password"` y aparecen enmascarados.
- La clave JWT no está presente en `appsettings.json` ni en archivos versionados; se obtiene desde User Secrets.
- No se registraron tokens completos, claves JWT ni contraseñas temporales en capturas o documentación.
- Los contratos omiten `PasswordHash`.
- Las denegaciones se probaron directamente contra la API y no solo ocultando opciones en Blazor.

## Base de datos y migraciones

La Etapa I no cambió el esquema después de `InitialUsersAndRoles`. `database/SGECAR.sql` conserva la definición vigente de `Roles`, `Usuarios`, sus índices y la relación foránea; no se generó una migración innecesaria.

## Resultado final

Todas las pruebas de cierre finalizaron en PASS. No quedó ningún fallo funcional conocido dentro del alcance de la Etapa I.
