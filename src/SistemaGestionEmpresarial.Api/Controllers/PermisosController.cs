using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Api.Entities;
using SistemaGestionEmpresarial.Api.Services;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Controllers;

[ApiController]
[Route("api/permisos")]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status403Forbidden)]
public sealed class PermisosController : ControllerBase
{
    private readonly IUsuarioActual _usuarioActual;
    private readonly AppDbContext _dbContext;

    public PermisosController(IUsuarioActual usuarioActual, AppDbContext dbContext)
    {
        _usuarioActual = usuarioActual;
        _dbContext = dbContext;
    }

    [HttpGet("mios")]
    [ProducesResponseType(typeof(PermisosResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PermisosResponse>> Mios(CancellationToken cancellationToken)
    {
        var permisos = await _usuarioActual.ObtenerPermisosAsync(cancellationToken);

        return new PermisosResponse
        {
            Usuario = _usuarioActual.NombreUsuario ?? string.Empty,
            Rol = _usuarioActual.Rol ?? string.Empty,
            Permisos = permisos.OrderBy(permiso => permiso, StringComparer.Ordinal).ToArray()
        };
    }

    [HttpGet("catalogo")]
    [Permiso(Permisos.GestionarRoles)]
    [ProducesResponseType(typeof(IReadOnlyList<RolPermisosResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RolPermisosResponse>>> Catalogo(CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(rol => rol.Nombre)
            .ToListAsync(cancellationToken);

        return roles.Select(rol => new RolPermisosResponse
        {
            Rol = rol.Nombre,
            Permisos = ObtenerPermisos(rol)
        }).ToArray();
    }

    private static string[] ObtenerPermisos(Rol rol)
    {
        var permisos = new List<string>();

        if (rol.PuedeConsultar) permisos.Add(Permisos.Consultar);
        if (rol.PuedeAgregar) permisos.Add(Permisos.Agregar);
        if (rol.PuedeModificar) permisos.Add(Permisos.Modificar);
        if (rol.PuedeEliminar) permisos.Add(Permisos.Eliminar);
        if (rol.PuedeGestionarUsuarios) permisos.Add(Permisos.GestionarUsuarios);
        if (rol.PuedeGestionarRoles) permisos.Add(Permisos.GestionarRoles);

        return permisos.ToArray();
    }
}
