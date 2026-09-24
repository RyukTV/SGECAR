using System.ComponentModel.DataAnnotations;

namespace SistemaGestionEmpresarial.Contracts;

public sealed class ActualizarRolRequest
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre del rol no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    public bool PuedeAgregar { get; set; }
    public bool PuedeModificar { get; set; }
    public bool PuedeEliminar { get; set; }
    public bool PuedeConsultar { get; set; }
    public bool PuedeGestionarUsuarios { get; set; }
    public bool PuedeGestionarRoles { get; set; }
}
