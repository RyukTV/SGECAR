using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Api.Services;
using SistemaGestionEmpresarial.Contracts;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Controllers;

[ApiController]
[Route("api/reportes/productos")]
public sealed class ReporteProductosController(ReporteProductosPdfService pdfService) : ControllerBase
{
    [HttpPost("pdf")]
    [Permiso(Permisos.Consultar)]
    [Produces("application/pdf")]
    public IActionResult DescargarPdf([FromBody] ReporteProductosPdfRequest request)
    {
        if (request.FechaGeneracion == default ||
            string.IsNullOrWhiteSpace(request.Criterios) ||
            request.Productos is null || request.Productos.Count > 10000)
            return BadRequest(new MensajeResponse { Message = "Los datos del reporte son inválidos." });

        var nombreUsuario = User.FindFirstValue(ClaimTypes.GivenName)
            ?? User.Identity?.Name
            ?? "Usuario";
        var pdf = pdfService.Generar(request, nombreUsuario);
        return File(pdf, "application/pdf", $"ReporteProductos_{request.FechaGeneracion:yyyy-MM-dd}.pdf");
    }
}
