namespace SistemaGestionEmpresarial.Api.Entities;

public sealed class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int CategoriaId { get; set; }
    public CategoriaProducto Categoria { get; set; } = null!;
}
