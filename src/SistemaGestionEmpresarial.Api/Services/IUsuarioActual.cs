using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Services;

/// <summary>
/// Acceso al usuario autenticado desde la capa de servicios, para comprobar permisos en reglas
/// de negocio que el atributo <c>[Permiso]</c> del Controller no puede expresar (por ejemplo,
/// cuando el permiso exigido depende de los datos que se están modificando).
/// </summary>
public interface IUsuarioActual
{
    bool EstaAutenticado { get; }
    string? NombreUsuario { get; }
    string? Rol { get; }

    Task<IReadOnlySet<string>> ObtenerPermisosAsync(CancellationToken cancellationToken = default);

    Task<bool> TienePermisoAsync(string permiso, CancellationToken cancellationToken = default);

    /// <summary>
    /// Comprueba el permiso y lanza <c>PermisoDenegadoException</c> si falta, lo que el
    /// middleware traduce a un 403 con el mismo cuerpo JSON que produce <c>[Permiso]</c>.
    /// </summary>
    Task ExigirPermisoAsync(string permiso, CancellationToken cancellationToken = default);
}
