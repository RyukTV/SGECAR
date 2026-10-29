namespace SistemaGestionEmpresarial.Contracts;

// Instantánea del reporte ya generado en el navegador.
public sealed class ReporteProductosPdfRequest
{
    public DateTime FechaGeneracion { get; set; }
    public string Criterios { get; set; } = string.Empty;
    public List<ProductoResponse> Productos { get; set; } = [];
}
