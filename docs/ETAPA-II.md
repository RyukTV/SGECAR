# Etapa II: Clientes, Productos, Categorías, Usuarios y Reporte

## Objetivo

Esta etapa incorpora los módulos de mantenimiento de clientes y productos y completa las consultas avanzadas del mantenimiento de usuarios, reutilizando la arquitectura de la solución: Blazor WebAssembly consume la API mediante HTTP/JSON; la API aplica autenticación y autorización por permisos, ejecuta la lógica de servicios y persiste los datos con Entity Framework Core y SQL Server.

---

## 1. Módulo de Clientes

Cada cliente contiene los campos `Id`, `Nombre`, `Apellido`, `Telefono`, `Correo` y `Direccion`. La dirección de correo es obligatoria y única.

### Endpoints en `/api/clientes`:
- `GET /api/clientes`: lista clientes y admite filtros opcionales por `id`, `buscar` y `correo`.
- `GET /api/clientes/{id}`: obtiene un cliente por identificador.
- `POST /api/clientes`: crea un nuevo cliente.
- `PUT /api/clientes/{id}`: actualiza un cliente existente.
- `DELETE /api/clientes/{id}`: elimina un cliente.

La página `/administracion/clientes` permite consultar, filtrar, registrar, modificar y eliminar clientes, con limpieza independiente de filtros y formulario. Requiere el permiso `GestionarClientes`.

El formulario separa el prefijo (`809`, `829` o `849`) de los siete dígitos locales. La interfaz inserta automáticamente el guion tras los tres primeros dígitos y guarda el valor completo como `809-555-1234` en `Cliente.Telefono`. Al editar un número válido existente, recupera el prefijo y el número local. El servicio rechaza cualquier teléfono fuera del formato `(809|829|849)-XXX-XXXX`; no se necesita una columna ni migración adicional.

El formulario exige nombre, apellido, teléfono, correo válido y dirección. La API repite las validaciones y rechaza correos duplicados con `409 Conflict`. Los filtros de clientes por ID, nombre/apellido y correo se pueden combinar.

---

## 2. Módulo de Productos

Cada producto contiene los campos:
- **`Id`**: identificador único autoincremental (PK).
- **`Codigo`**: código del producto (obligatorio, máximo 50 caracteres, índice único).
- **`Nombre`**: nombre comercial del producto (obligatorio, máximo 150 caracteres).
- **`Precio`**: valor decimal con precisión (18, 2), obligatorio y mayor a 0 (`Precio > 0`).
- **`Stock`**: cantidad en existencias entera, obligatoria y mayor o igual a 0 (`Stock >= 0`).
- **`CategoriaId`**: referencia obligatoria a `CategoriasProducto`; la categoría se selecciona del catálogo.

### Operaciones CRUD y Endpoints en `/api/productos`:
- `GET /api/productos`: lista productos con filtros opcionales `codigoOId`, `nombre` y `categoriaId`, combinables. Requiere permiso `Consultar`.
- `GET /api/productos/categorias-disponibles`: lista las categorías para los selectores. Requiere permiso `Consultar`.
- `GET /api/productos/{id}`: obtiene el detalle de un producto por su identificador. Requiere permiso `Consultar`.
- `POST /api/productos`: registra un nuevo producto validando unicidad de código y reglas de negocio. Requiere permiso `Agregar`.
- `PUT /api/productos/{id}`: actualiza un producto existente validando unicidad de código entre otros registros. Requiere permiso `Modificar`.
- `DELETE /api/productos/{id}`: elimina un producto del catálogo. Requiere permiso `Eliminar`.

### Control de Permisos en Productos:
Se utilizan los permisos ya existentes en el sistema:
- **Administrador**: posee todos los permisos (`Consultar`, `Agregar`, `Modificar`, `Eliminar`). Puede ejecutar todas las operaciones CRUD.
- **Supervisor**: posee `Consultar` y `Modificar`. Puede consultar, filtrar y modificar productos; no puede agregar ni eliminar (los botones se ocultan/bloquean o se indica falta de permisos).
- **Ejecutor**: posee `Consultar` y `Agregar`. Puede consultar, filtrar y agregar nuevos productos; no puede modificar ni eliminar (los botones se ocultan/bloquean o se indica falta de permisos).

### Validaciones implementadas:
- Código obligatorio y único (evita duplicados en base de datos y a nivel de servicio).
- Nombre obligatorio.
- Precio mayor que 0.
- Stock mayor o igual a 0.
- Categoría obligatoria.

### Interfaz de Usuario (`/administracion/productos`):
- **Buscadores y filtros**:
  - Buscador por ID o código.
  - Buscador por nombre.
  - ComboBox dinámico por Categoría.
  - Botón de "Buscar".
  - Botón de "Limpiar filtros".
- **Formulario interactivo**:
  - Validaciones de formulario con DataAnnotations.
  - Botón "Guardar" adaptado al modo creación/edición y permisos del rol.
  - Botón "Limpiar campos".
  - Botón "Cancelar".
- **Tabla de Productos**:
  - Columnas: ID, Código, Nombre, Categoría, Precio formateado en moneda y Stock con indicador visual (agotado, bajo stock, disponible).
  - Acciones condicionadas por permisos: botón "Editar" (requiere Modificar), botón "Eliminar" con diálogo de confirmación (requiere Eliminar).

---

## 3. Categorías de Productos

`/administracion/categorias-productos` permite consultar, crear, modificar y eliminar categorías. El nombre es obligatorio, único y admite hasta 100 caracteres. Una categoría con productos relacionados no se puede eliminar: `DELETE /api/categorias-productos/{id}` responde `409 Conflict`. `Productos.CategoriaId` tiene una FK con borrado restringido. El permiso `GestionarCategorias` habilita el mantenimiento para Administrador y roles personalizados que lo tengan; Supervisor y Ejecutor no lo reciben inicialmente. Los roles personalizados deben iniciar sesión nuevamente para actualizar el JWT.

Los permisos de Productos permanecen por acción: `Consultar`, `Agregar`, `Modificar` y `Eliminar`. Los permisos administrativos `GestionarUsuarios`, `GestionarRoles`, `GestionarClientes` y `GestionarCategorias` se asignan a roles, sin autorizar por nombre de rol codificado.

## 4. Reporte de Productos

`/reportes/productos` genera un reporte sin filtros o filtrado por ID/código, nombre y categoría, con criterios combinables. El documento muestra entidad, criterios aplicados, fecha de generación, usuario autenticado que lo descargó, cantidad de registros y tabla. «Descargar PDF» envía la instantánea mostrada a la API, que genera únicamente ese documento en A4 vertical.

`POST /api/reportes/productos/pdf` requiere `Consultar` y devuelve `application/pdf`. La API genera el documento con QuestPDF 2026.9.1 usando las filas y criterios ya mostrados en Blazor; el nombre del usuario procede del token autenticado. El PDF contiene solamente el reporte y su tabla, sin navegación ni controles de pantalla.

---

## 5. Módulo de Usuarios: búsqueda y filtros

El CRUD de usuarios de la Etapa I se mantiene sin cambios (crear, editar, eliminar, cambiar rol y estado, y todas sus validaciones, incluida la protección del usuario autenticado). Esta etapa agrega consulta avanzada. No requiere cambios de esquema ni migración.

### Endpoint `GET /api/usuarios`
Todos los parámetros son opcionales y, cuando se envían varios, se combinan con AND. Requiere el permiso `GestionarUsuarios`.
- `id`: devuelve únicamente el usuario con ese identificador.
- `buscar`: texto contenido en usuario, nombre completo o nombre del rol (comportamiento previo).
- `rolId`: usuarios que pertenecen a ese rol.
- `activo`: `true` para activos, `false` para inactivos; si se omite, devuelve ambos.

Los valores de `rolId` provienen de los roles guardados en la base de datos, mediante `GET /api/usuarios/roles-disponibles`.

### Interfaz de Usuario (`/administracion/usuarios`)
- **Filtros**: ID, texto (usuario / nombre / rol), ComboBox de Rol alimentado con los roles de la base de datos, y ComboBox de Estado con `Todos`, `Activos` e `Inactivos`. Los filtros se pueden combinar.
- **Botón "Buscar"** aplica los filtros y **"Limpiar filtros"** los restablece y recarga la lista completa.
- **Botón "Limpiar campos"** en el formulario de creación/edición vacía los campos (el estado vuelve a Activo) y los mensajes de validación, sin cerrar el formulario ni guardar.
- La tabla muestra la columna ID.
- La API mantiene las validaciones de usuario, rol y contraseña, rechaza nombres de usuario duplicados y protege al usuario autenticado contra su propia eliminación o desactivación.

---

## 6. Base de Datos

- La migración `20261027175429_AddProductos` crea la tabla `Productos` con sus restricciones y el índice único `IX_Productos_Codigo`.
- La migración `20261028181644_AddProductCategoriesAndPermission` conserva los productos existentes, crea `CategoriasProducto`, relaciona cada producto mediante `CategoriaId` y agrega `PuedeGestionarCategorias` a `Roles`.
- El script SQL idempotente general se actualizó en [`database/SGECAR.sql`](../database/SGECAR.sql).
- El script contiene `Roles`, `Usuarios`, `Clientes`, `CategoriasProducto` y `Productos`, con sus índices únicos, permisos y claves foráneas. No hay cambios de esquema para la máscara telefónica.
