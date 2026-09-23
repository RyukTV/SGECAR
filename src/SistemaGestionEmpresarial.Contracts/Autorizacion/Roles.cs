namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

/// <summary>
/// Nombres de los roles definidos en la práctica. Coinciden con la columna <c>Roles.Nombre</c>
/// de la base de datos y con el claim de rol que viaja dentro del JWT.
/// </summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Supervisor = "Supervisor";
    public const string Ejecutor = "Ejecutor";

    public static IReadOnlyList<string> Todos { get; } = new[]
    {
        Administrador,
        Supervisor,
        Ejecutor
    };
}
