namespace SistemaGestionEmpresarial.Api.Authorization;

/// <summary>
/// La lanza un Service cuando comprueba un permiso por su cuenta, fuera del filtro de
/// autorización de los Controllers. <c>MiddlewareDePermisoDenegado</c> la traduce a un 403.
/// </summary>
public sealed class PermisoDenegadoException : Exception
{
    public PermisoDenegadoException(string? rol, string permisoRequerido)
        : base($"El rol '{rol ?? "(anónimo)"}' no tiene el permiso '{permisoRequerido}'.")
    {
        Rol = rol;
        PermisoRequerido = permisoRequerido;
    }

    public string? Rol { get; }
    public string PermisoRequerido { get; }
}
