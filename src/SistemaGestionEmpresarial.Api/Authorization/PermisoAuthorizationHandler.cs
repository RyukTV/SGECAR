using Microsoft.AspNetCore.Authorization;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Authorization;

/// <summary>
/// Comprueba en el servidor que el rol del token incluya el permiso exigido por el endpoint.
/// Si el usuario no está autenticado o su rol no tiene el permiso, el requisito no se satisface
/// y la petición termina en 403: ocultar el botón en Blazor nunca es suficiente.
/// </summary>
public sealed class PermisoAuthorizationHandler : AuthorizationHandler<PermisoRequirement>
{
    private readonly IProveedorDePermisos _proveedorDePermisos;
    private readonly ILogger<PermisoAuthorizationHandler> _logger;

    public PermisoAuthorizationHandler(
        IProveedorDePermisos proveedorDePermisos,
        ILogger<PermisoAuthorizationHandler> logger)
    {
        _proveedorDePermisos = proveedorDePermisos;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermisoRequirement requirement)
    {
        var rol = context.User.ObtenerRol();

        if (rol is null)
        {
            return;
        }

        var permisos = await _proveedorDePermisos.ObtenerPermisosAsync(rol);

        if (permisos.Contains(requirement.Permiso))
        {
            context.Succeed(requirement);
            return;
        }

        _logger.LogWarning(
            "Acceso denegado: el rol '{Rol}' intentó una operación que exige el permiso '{Permiso}'.",
            rol,
            requirement.Permiso);
    }
}
