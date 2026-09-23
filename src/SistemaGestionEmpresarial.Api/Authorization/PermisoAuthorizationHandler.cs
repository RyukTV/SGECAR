using Microsoft.AspNetCore.Authorization;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Authorization;

public sealed class PermisoAuthorizationHandler : AuthorizationHandler<PermisoRequirement>
{
    private readonly ILogger<PermisoAuthorizationHandler> _logger;

    public PermisoAuthorizationHandler(ILogger<PermisoAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermisoRequirement requirement)
    {
        if (context.User.TienePermiso(requirement.Permiso))
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "Acceso denegado: el rol '{Rol}' intentó una operación que exige el permiso '{Permiso}'.",
                context.User.ObtenerRol(),
                requirement.Permiso);
        }

        return Task.CompletedTask;
    }
}
