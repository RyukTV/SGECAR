using Microsoft.EntityFrameworkCore;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Api.Entities;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public sealed class RolesService : IRolesService
{
    private readonly AppDbContext _dbContext;

    public RolesService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RolResponse>> ObtenerTodosAsync(
        string? buscar,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Roles.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            query = query.Where(rol => rol.Nombre.Contains(termino));
        }

        return await query
            .OrderBy(rol => rol.Nombre)
            .Select(rol => new RolResponse
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                PuedeAgregar = rol.PuedeAgregar,
                PuedeModificar = rol.PuedeModificar,
                PuedeEliminar = rol.PuedeEliminar,
                PuedeConsultar = rol.PuedeConsultar,
                PuedeGestionarUsuarios = rol.PuedeGestionarUsuarios,
                PuedeGestionarRoles = rol.PuedeGestionarRoles
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RolResponse?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbContext.Roles
            .AsNoTracking()
            .Where(rol => rol.Id == id)
            .Select(rol => new RolResponse
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                PuedeAgregar = rol.PuedeAgregar,
                PuedeModificar = rol.PuedeModificar,
                PuedeEliminar = rol.PuedeEliminar,
                PuedeConsultar = rol.PuedeConsultar,
                PuedeGestionarUsuarios = rol.PuedeGestionarUsuarios,
                PuedeGestionarRoles = rol.PuedeGestionarRoles
            })
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ResultadoCrud<RolResponse>> CrearAsync(
        CrearRolRequest request,
        CancellationToken cancellationToken = default)
    {
        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return ResultadoCrud<RolResponse>.Validacion("El nombre del rol es obligatorio.");

        if (await _dbContext.Roles.AnyAsync(rol => rol.Nombre == nombre, cancellationToken))
            return ResultadoCrud<RolResponse>.Conflicto("Ya existe un rol con ese nombre.");

        var rol = new Rol { Nombre = nombre };
        AplicarPermisos(rol, request.PuedeAgregar, request.PuedeModificar, request.PuedeEliminar,
            request.PuedeConsultar, request.PuedeGestionarUsuarios, request.PuedeGestionarRoles);

        _dbContext.Roles.Add(rol);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ResultadoCrud<RolResponse>.Exito(Mapear(rol), "Rol creado correctamente.");
    }

    public async Task<ResultadoCrud<RolResponse>> ActualizarAsync(
        int id,
        ActualizarRolRequest request,
        CancellationToken cancellationToken = default)
    {
        var rol = await _dbContext.Roles.SingleOrDefaultAsync(entidad => entidad.Id == id, cancellationToken);
        if (rol is null)
            return ResultadoCrud<RolResponse>.NoEncontrado("El rol solicitado no existe.");

        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return ResultadoCrud<RolResponse>.Validacion("El nombre del rol es obligatorio.");

        if (await _dbContext.Roles.AnyAsync(
                entidad => entidad.Id != id && entidad.Nombre == nombre,
                cancellationToken))
            return ResultadoCrud<RolResponse>.Conflicto("Ya existe un rol con ese nombre.");

        rol.Nombre = nombre;
        AplicarPermisos(rol, request.PuedeAgregar, request.PuedeModificar, request.PuedeEliminar,
            request.PuedeConsultar, request.PuedeGestionarUsuarios, request.PuedeGestionarRoles);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ResultadoCrud<RolResponse>.Exito(Mapear(rol), "Rol actualizado correctamente.");
    }

    public async Task<ResultadoCrud<bool>> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var rol = await _dbContext.Roles.SingleOrDefaultAsync(entidad => entidad.Id == id, cancellationToken);
        if (rol is null)
            return ResultadoCrud<bool>.NoEncontrado("El rol solicitado no existe.");

        if (await _dbContext.Usuarios.AnyAsync(usuario => usuario.RolId == id, cancellationToken))
            return ResultadoCrud<bool>.Conflicto("No se puede eliminar el rol porque tiene usuarios asignados.");

        _dbContext.Roles.Remove(rol);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ResultadoCrud<bool>.Exito(true, "Rol eliminado correctamente.");
    }

    private static void AplicarPermisos(
        Rol rol,
        bool agregar,
        bool modificar,
        bool eliminar,
        bool consultar,
        bool gestionarUsuarios,
        bool gestionarRoles)
    {
        rol.PuedeAgregar = agregar;
        rol.PuedeModificar = modificar;
        rol.PuedeEliminar = eliminar;
        rol.PuedeConsultar = consultar;
        rol.PuedeGestionarUsuarios = gestionarUsuarios;
        rol.PuedeGestionarRoles = gestionarRoles;
    }

    private static RolResponse Mapear(Rol rol) => new()
    {
        Id = rol.Id,
        Nombre = rol.Nombre,
        PuedeAgregar = rol.PuedeAgregar,
        PuedeModificar = rol.PuedeModificar,
        PuedeEliminar = rol.PuedeEliminar,
        PuedeConsultar = rol.PuedeConsultar,
        PuedeGestionarUsuarios = rol.PuedeGestionarUsuarios,
        PuedeGestionarRoles = rol.PuedeGestionarRoles
    };
}
