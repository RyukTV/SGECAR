# Etapa II: mantenimiento de clientes

## Objetivo

Esta etapa incorpora el mantenimiento de clientes reutilizando la arquitectura existente: Blazor WebAssembly consume la API mediante HTTP/JSON; la API aplica autorización, ejecuta la lógica del servicio y persiste los datos con Entity Framework Core y SQL Server.

## Módulo de clientes

Cada cliente contiene los campos `Id`, `Nombre`, `Apellido`, `Telefono`, `Correo` y `Direccion`. La dirección de correo es obligatoria y única.

La API expone las siguientes operaciones en `/api/clientes`:

- `GET /api/clientes`: lista clientes y admite los filtros opcionales `id`, `buscar` y `correo`.
- `GET /api/clientes/{id}`: obtiene un cliente por identificador.
- `POST /api/clientes`: crea un cliente.
- `PUT /api/clientes/{id}`: actualiza un cliente.
- `DELETE /api/clientes/{id}`: elimina un cliente.

La página `/administracion/clientes` permite listar, filtrar, crear, editar y eliminar clientes. También permite limpiar por separado el formulario y los filtros.

## Autorización

Todas las operaciones del módulo requieren el permiso `GestionarClientes`. El rol Administrador recibe este permiso en la migración; Supervisor y Ejecutor no lo reciben. Los roles personalizados pueden habilitarlo desde el mantenimiento de roles.

La autorización depende del permiso y no del nombre del rol, por lo que un rol personalizado con `GestionarClientes` puede utilizar el módulo.

## Base de datos

La migración `AddClientesAndGestionarClientes` crea la tabla `Clientes`, su índice único de correo y la columna de permiso en `Roles`. El script idempotente actualizado se encuentra en `database/SGECAR.sql`.

## Verificación

Se verificaron el CRUD completo, los filtros individuales y combinados, las validaciones, el correo duplicado, los códigos 401/403, un rol personalizado autorizado y las regresiones principales de salud, usuarios y roles. También se comprobó manualmente el flujo completo desde Blazor WebAssembly.

## Pendiente para etapas posteriores

Quedan deliberadamente fuera de esta etapa el módulo de productos, los filtros adicionales de usuarios y cualquier implementación con ADO.NET.
