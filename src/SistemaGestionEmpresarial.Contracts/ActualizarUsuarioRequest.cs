using System.ComponentModel.DataAnnotations;

namespace SistemaGestionEmpresarial.Contracts;

public sealed class ActualizarUsuarioRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre de usuario no puede superar 100 caracteres.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre completo no puede superar 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol.")]
    public int RolId { get; set; }

    public bool Activo { get; set; }
    public string? NuevaPassword { get; set; }
}
