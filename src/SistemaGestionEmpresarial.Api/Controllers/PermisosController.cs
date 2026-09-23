using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Api.Authorization;
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

    public PermisosController(IUsuarioActual usuarioActual)
    {
        _usuarioActual = usuarioActual;
    }

    /// <summary>
    /// Permisos efectivos del usuario autenticado, resueltos por el servidor. El cliente los usa
    /// para armar el menú sin duplicar la regla y para contrastar lo que muestra en pantalla.
    /// </summary>
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

    /// <summary>Matriz completa rol → permisos. Solo para quien puede gestionar roles.</summary>
    [HttpGet("catalogo")]
    [Permiso(Permisos.GestionarRoles)]
    [ProducesResponseType(typeof(IReadOnlyList<RolPermisosResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<RolPermisosResponse>> Catalogo() =>
        Roles.Todos
            .Select(rol => new RolPermisosResponse
            {
                Rol = rol,
                Permisos = CatalogoDePermisos.ObtenerPermisos(rol)
                    .OrderBy(permiso => permiso, StringComparer.Ordinal)
                    .ToArray()
            })
            .ToArray();
}
