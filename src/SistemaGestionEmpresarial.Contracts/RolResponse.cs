namespace SistemaGestionEmpresarial.Contracts;

public sealed class RolResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool PuedeAgregar { get; set; }
    public bool PuedeModificar { get; set; }
    public bool PuedeEliminar { get; set; }
    public bool PuedeConsultar { get; set; }
    public bool PuedeGestionarUsuarios { get; set; }
    public bool PuedeGestionarRoles { get; set; }
}
