using Microsoft.AspNetCore.Authorization;

namespace SistemaGestionEmpresarial.Api.Authorization;

/// <summary>Exigencia de un permiso concreto sobre el usuario autenticado.</summary>
public sealed class PermisoRequirement : IAuthorizationRequirement
{
    public PermisoRequirement(string permiso)
    {
        Permiso = permiso;
    }

    public string Permiso { get; }
}
