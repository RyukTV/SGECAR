namespace SistemaGestionEmpresarial.Api.Entities;

public sealed class CategoriaProducto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public ICollection<Producto> Productos { get; set; } = [];
}
