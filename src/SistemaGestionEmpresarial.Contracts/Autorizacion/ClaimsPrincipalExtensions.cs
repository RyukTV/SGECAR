using System.Security.Claims;

namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

/// <summary>
/// Lectura del rol que viaja en el JWT. Se comparte entre la API y Blazor porque ambos leen
/// el mismo token y deben interpretarlo igual.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Rol del usuario autenticado, o <c>null</c> si el principal es anónimo o no trae rol.
    /// Se consultan los tres nombres de claim que puede producir el token: el tipo estándar de
    /// .NET, el "role" corto del JWT y el "rol" adicional que emite <c>AuthService</c>.
    /// </summary>
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

    /// <summary>Indica si el rol del usuario incluye el permiso solicitado.</summary>
    public static bool TienePermiso(this ClaimsPrincipal? usuario, string permiso) =>
        CatalogoDePermisos.TienePermiso(usuario.ObtenerRol(), permiso);
}
