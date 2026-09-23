namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

/// <summary>
/// Nombres de las políticas de autorización. Se comparten para que la política que la API
/// registra y la que Blazor evalúa sean literalmente la misma cadena.
/// </summary>
/// <remarks>
/// Las constantes existen porque <c>[Authorize(Policy = ...)]</c> solo admite valores conocidos
/// en compilación; <see cref="Nombre" /> cubre los usos dinámicos, como recorrer
/// <see cref="Permisos.Todos" /> al registrar las políticas.
/// </remarks>
public static class PoliticasDePermiso
{
    private const string Prefijo = "Permiso:";

    public const string Consultar = Prefijo + Permisos.Consultar;
    public const string Agregar = Prefijo + Permisos.Agregar;
    public const string Modificar = Prefijo + Permisos.Modificar;
    public const string Eliminar = Prefijo + Permisos.Eliminar;
    public const string GestionarUsuarios = Prefijo + Permisos.GestionarUsuarios;
    public const string GestionarRoles = Prefijo + Permisos.GestionarRoles;

    /// <summary>Nombre de la política que exige un permiso concreto, por ejemplo "Permiso:Eliminar".</summary>
    public static string Nombre(string permiso) => Prefijo + permiso;
}
