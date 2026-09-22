namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

/// <summary>Una fila de la matriz rol → permisos.</summary>
public sealed class RolPermisosResponse
{
    public string Rol { get; set; } = string.Empty;
    public IReadOnlyList<string> Permisos { get; set; } = Array.Empty<string>();
}
