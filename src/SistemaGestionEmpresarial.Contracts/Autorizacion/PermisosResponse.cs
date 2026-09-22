namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

/// <summary>
/// Permisos efectivos del usuario autenticado, tal como los resuelve la API.
/// Permite que el cliente confirme contra el servidor lo que muestra en pantalla.
/// </summary>
public sealed class PermisosResponse
{
    public string Usuario { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public IReadOnlyList<string> Permisos { get; set; } = Array.Empty<string>();
}
