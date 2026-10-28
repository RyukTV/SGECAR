# Etapa II: Mantenimiento de Clientes y Productos

## Objetivo

Esta etapa incorpora los módulos de mantenimiento de clientes y productos reutilizando la arquitectura de la solución: Blazor WebAssembly consume la API mediante HTTP/JSON; la API aplica autenticación y autorización por permisos, ejecuta la lógica de servicios y persiste los datos con Entity Framework Core y SQL Server.

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

---

## 2. Módulo de Productos

Cada producto contiene los campos:
- **`Id`**: identificador único autoincremental (PK).
- **`Codigo`**: código del producto (obligatorio, máximo 50 caracteres, índice único).
- **`Nombre`**: nombre comercial del producto (obligatorio, máximo 150 caracteres).
- **`Precio`**: valor decimal con precisión (18, 2), obligatorio y mayor a 0 (`Precio > 0`).
- **`Stock`**: cantidad en existencias entera, obligatoria y mayor o igual a 0 (`Stock >= 0`).
- **`Categoria`**: rubro o categoría a la que pertenece el producto (obligatorio, máximo 100 caracteres).

### Operaciones CRUD y Endpoints en `/api/productos`:
- `GET /api/productos`: lista productos con soporte de filtros opcionales (`id`, `buscar` por nombre/código, `categoria`). Requiere permiso `Consultar`.
- `GET /api/productos/categorias`: lista las categorías existentes sin duplicados para alimentar el ComboBox/filtro de la interfaz. Requiere permiso `Consultar`.
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
  - Buscador por ID.
  - Buscador por Nombre / Código.
  - ComboBox dinámico por Categoría.
  - Botón de "Buscar".
  - Botón de "Limpiar filtros".
- **Formulario interactivo**:
  - Validaciones en tiempo real con DataAnnotations.
  - Botón "Guardar" adaptado al modo creación/edición y permisos del rol.
  - Botón "Limpiar campos".
  - Botón "Cancelar".
- **DataGridView / Tabla de Productos**:
  - Columnas: ID, Código, Nombre, Categoría, Precio formateado en moneda y Stock con indicador visual (agotado, bajo stock, disponible).
  - Acciones condicionadas por permisos: botón "Editar" (requiere Modificar), botón "Eliminar" con diálogo de confirmación (requiere Eliminar).

---

## 3. Módulo de Usuarios: búsqueda y filtros

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

---

## 4. Base de Datos

- La migración `20261027175429_AddProductos` crea la tabla `Productos` con sus restricciones y el índice único `IX_Productos_Codigo`.
- El script SQL idempotente general se actualizó en [`database/SGECAR.sql`](../database/SGECAR.sql).
