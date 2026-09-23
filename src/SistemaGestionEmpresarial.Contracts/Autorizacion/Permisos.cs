namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

/// <summary>
/// Acciones que el sistema sabe autorizar. Cada nombre corresponde a una columna booleana
/// de la entidad <c>Rol</c>, de modo que el catálogo pueda alimentarse desde la base de datos
/// sin cambiar los nombres usados por la API ni por Blazor.
/// </summary>
public static class Permisos
{
    public const string Consultar = "Consultar";
    public const string Agregar = "Agregar";
    public const string Modificar = "Modificar";
    public const string Eliminar = "Eliminar";
    public const string GestionarUsuarios = "GestionarUsuarios";
    public const string GestionarRoles = "GestionarRoles";

    public static IReadOnlyList<string> Todos { get; } = new[]
    {
        Consultar,
        Agregar,
        Modificar,
        Eliminar,
        GestionarUsuarios,
        GestionarRoles
    };

    /// <summary>
    /// Texto en minúsculas para redactar mensajes dirigidos al usuario,
    /// por ejemplo "No cuenta con el permiso necesario para <c>eliminar registros</c>".
    /// </summary>
    public static string Describir(string permiso) => permiso switch
    {
        Consultar => "consultar registros",
        Agregar => "agregar registros",
        Modificar => "modificar registros",
        Eliminar => "eliminar registros",
        GestionarUsuarios => "gestionar usuarios",
        GestionarRoles => "gestionar roles",
        _ => "realizar esta acción"
    };
}
