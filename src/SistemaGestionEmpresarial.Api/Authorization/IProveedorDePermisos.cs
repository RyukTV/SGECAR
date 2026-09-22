namespace SistemaGestionEmpresarial.Api.Authorization;

/// <summary>
/// Resuelve los permisos de un rol. Es el único punto que hay que sustituir cuando los permisos
/// pasen a leerse de las columnas booleanas de la tabla <c>Roles</c> en SQL Server: el resto de
/// la autorización (políticas, atributos, handler y cliente Blazor) queda sin cambios.
/// </summary>
public interface IProveedorDePermisos
{
    Task<IReadOnlySet<string>> ObtenerPermisosAsync(string? rol, CancellationToken cancellationToken = default);
}
