namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

/// <summary>
/// Matriz rol → permisos de la Etapa I. Es la única fuente de verdad compartida por la API y
/// por Blazor, de modo que el botón que se oculta en el cliente y la regla que rechaza la
/// petición en el servidor nunca puedan divergir.
/// </summary>
/// <remarks>
/// Los valores reproducen el seed de <c>DatabaseSeeder</c>. Cuando la tabla <c>Roles</c> pase a
/// administrarse desde la aplicación, la API debe sustituir este catálogo por un proveedor que
/// lea las columnas booleanas de la entidad <c>Rol</c> (ver <c>IProveedorDePermisos</c>);
/// los nombres de permiso no cambian, por lo que el resto del código queda intacto.
/// </remarks>
public static class CatalogoDePermisos
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> PermisosPorRol =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [Roles.Administrador] = Conjunto(
                Permisos.Consultar,
                Permisos.Agregar,
                Permisos.Modificar,
                Permisos.Eliminar,
                Permisos.GestionarUsuarios,
                Permisos.GestionarRoles),

            // El Supervisor consulta y modifica, pero no agrega ni elimina.
            [Roles.Supervisor] = Conjunto(
                Permisos.Consultar,
                Permisos.Modificar),

            // El Ejecutor consulta y agrega, pero no modifica ni elimina.
            [Roles.Ejecutor] = Conjunto(
                Permisos.Consultar,
                Permisos.Agregar)
        };

    private static readonly IReadOnlySet<string> SinPermisos =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Permisos del rol indicado. Un rol nulo, vacío o desconocido no tiene ningún permiso:
    /// la autorización siempre falla de forma cerrada.
    /// </summary>
    public static IReadOnlySet<string> ObtenerPermisos(string? rol)
    {
        if (string.IsNullOrWhiteSpace(rol))
        {
            return SinPermisos;
        }

        return PermisosPorRol.TryGetValue(rol.Trim(), out var permisos)
            ? permisos
            : SinPermisos;
    }

    public static bool TienePermiso(string? rol, string permiso) =>
        ObtenerPermisos(rol).Contains(permiso);

    private static IReadOnlySet<string> Conjunto(params string[] permisos) =>
        new HashSet<string>(permisos, StringComparer.OrdinalIgnoreCase);
}
