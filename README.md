# Sistema de Gestión Empresarial con Control de Acceso por Roles

Proyecto académico desarrollado por un equipo de 3 integrantes.

## Flujo de trabajo

```text
main
  ↑
Pull Request
  ↑
rama de trabajo
```

`main` representa siempre la versión revisada, aprobada, integrada y estable del proyecto. Nadie debe desarrollar directamente sobre esta rama.

## Regla principal

Ningún cambio entra a `main` sin revisión previa. Todo cambio debe realizarse en una rama de trabajo y proponerse mediante un Pull Request hacia `main`.

## Crear una rama

Toda rama nueva debe crearse desde `main` actualizado:

```bash
git switch main
git pull origin main
git switch -c tipo/nombre-rama
```

Ejemplo:

```bash
git switch main
git pull origin main
git switch -c feature/login
```

## Tipos de ramas

### `feature/<descripcion>`

Para desarrollar una funcionalidad nueva.

Ejemplos: `feature/cimiento`, `feature/login`, `feature/roles`, `feature/frontend`, `feature/modelo-dominio`.

### `fix/<descripcion>`

Para corregir un error o comportamiento incorrecto.

Ejemplos: `fix/login-validation`, `fix/api-connection`, `fix/roles-permissions`.

### `docs/<descripcion>`

Para cambios de documentación.

Ejemplos: `docs/documentacion-etapa1`, `docs/modelo-dominio`, `docs/readme`.

### `refactor/<descripcion>`

Para reorganizar o mejorar código existente sin cambiar su comportamiento funcional.

Ejemplos: `refactor/auth-service`, `refactor/project-structure`.

### `test/<descripcion>`

Para agregar o modificar pruebas como objetivo principal de la rama.

Ejemplos: `test/login`, `test/roles`.

### `chore/<descripcion>`

Para tareas de configuración o mantenimiento que no representan una funcionalidad.

Ejemplos: `chore/configuracion-inicial`, `chore/update-gitignore`.

## Commits

Los mensajes de commit deben ser cortos y claros. Ejemplos:

```text
feat: agrega login
fix: corrige validacion de usuario
docs: actualiza documentacion
refactor: reorganiza servicio de autenticacion
test: agrega pruebas de login
chore: configura repositorio
```

Se deben evitar mensajes imprecisos como `cambios`, `cosas`, `prueba`, `update` o `final`.

## Subir trabajo

```bash
git add .
git commit -m "tipo: descripcion"
git push -u origin nombre-rama
```

Ejemplo:

```bash
git push -u origin feature/login
```

Después se debe crear un Pull Request hacia `main`.

## Regla de integración a main

- No se hace push directo a `main`.
- No se hace merge directo sin revisión.
- Todo cambio entra mediante un Pull Request.
- El código debe ser revisado antes del merge.
- Si hay errores o cambios solicitados, deben corregirse en la misma rama.
- Solo después de la aprobación se integra el cambio a `main`.
- `main` debe mantenerse estable.
- No se permite hacer force push sobre `main`.

Antes de integrarse, todo cambio debe:

1. Estar terminado.
2. Estar probado por quien lo realizó.
3. Subirse a su rama de trabajo.
4. Abrir un Pull Request hacia `main`.
5. Ser revisado.
6. Corregirse si hace falta en la misma rama.
7. Ser aprobado antes del merge.

## Conflictos

Si dos integrantes necesitan modificar el mismo archivo o la misma funcionalidad, deben coordinarse antes de comenzar.

Si aparece un conflicto de Git importante, no debe resolverse a ciegas. Debe revisarse con el integrante responsable del código afectado.
