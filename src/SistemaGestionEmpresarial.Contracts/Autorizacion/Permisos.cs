namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

/// <summary>
/// Acciones autorizables del sistema. Los nombres coinciden con las columnas booleanas de Rol.
/// </summary>
public static class Permisos
{
    public const string ClaimType = "permiso";

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
