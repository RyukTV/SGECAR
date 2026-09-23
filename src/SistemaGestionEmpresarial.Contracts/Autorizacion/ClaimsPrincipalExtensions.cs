using System.Security.Claims;

namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

public static class ClaimsPrincipalExtensions
{
    public static string? ObtenerRol(this ClaimsPrincipal? usuario)
    {
        if (usuario?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var rol = usuario.FindFirst(ClaimTypes.Role)?.Value
            ?? usuario.FindFirst("role")?.Value
            ?? usuario.FindFirst("rol")?.Value;

        return string.IsNullOrWhiteSpace(rol) ? null : rol;
    }

    public static IReadOnlySet<string> ObtenerPermisos(this ClaimsPrincipal? usuario)
    {
        if (usuario?.Identity?.IsAuthenticated != true)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return usuario.FindAll(Permisos.ClaimType)
            .Select(claim => claim.Value)
            .Where(valor => !string.IsNullOrWhiteSpace(valor))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public static bool TienePermiso(this ClaimsPrincipal? usuario, string permiso) =>
        usuario.ObtenerPermisos().Contains(permiso);
}
