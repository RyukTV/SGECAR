using System.ComponentModel.DataAnnotations;

namespace SistemaGestionEmpresarial.Contracts;

public sealed class CrearCategoriaProductoRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}
