using System.ComponentModel.DataAnnotations;

namespace SistemaGestionEmpresarial.Contracts;

public sealed class ActualizarClienteRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El apellido no puede superar 100 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [MaxLength(30, ErrorMessage = "El teléfono no puede superar 30 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [MaxLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [MaxLength(250, ErrorMessage = "La dirección no puede superar 250 caracteres.")]
    public string Direccion { get; set; } = string.Empty;
}
