namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

/// <summary>
/// Cuerpo JSON que la API devuelve en un 401 o un 403. Sin esto ASP.NET Core responde con el
/// cuerpo vacío y el cliente no tendría nada que mostrarle al usuario.
/// Sigue la forma <c>Success</c>/<c>Message</c> ya usada por <see cref="LoginResponse"/>.
/// </summary>
public sealed class AccesoDenegadoResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    /// <summary>Permiso que exigía la operación rechazada, cuando la API puede determinarlo.</summary>
    public string? PermisoRequerido { get; set; }

    /// <summary>Rol con el que se intentó la operación.</summary>
    public string? Rol { get; set; }

    public static AccesoDenegadoResponse NoAutenticado() => new()
    {
        Success = false,
        Message = "Debe iniciar sesión para realizar esta acción."
    };

    public static AccesoDenegadoResponse PermisosInsuficientes(string? rol, string? permisoRequerido)
    {
        var accion = permisoRequerido is null
            ? "realizar esta acción"
            : Permisos.Describir(permisoRequerido);

        var mensaje = string.IsNullOrWhiteSpace(rol)
            ? $"No cuenta con el permiso necesario para {accion}."
            : $"El rol {rol} no cuenta con el permiso necesario para {accion}.";

        return new AccesoDenegadoResponse
        {
            Success = false,
            Message = mensaje,
            PermisoRequerido = permisoRequerido,
            Rol = rol
        };
    }
}
