using System.Net.Http.Json;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Web.Services;

public sealed class ReporteProductosApiService(HttpClient httpClient)
{
    public async Task<ResultadoApi<byte[]>> DescargarPdfAsync(
        ReporteProductosPdfRequest reporte, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "api/reportes/productos/pdf", reporte, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ResultadoApi<byte[]>.Fallo(
                    await RespuestaApiReader.LeerErrorAsync(
                        response, cancellationToken, "No fue posible generar el PDF."),
                    (int)response.StatusCode);

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return ResultadoApi<byte[]>.Correcto(bytes, codigoHttp: (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ResultadoApi<byte[]>.Fallo("No fue posible comunicarse con la API.");
        }
        catch (TaskCanceledException)
        {
            return ResultadoApi<byte[]>.Fallo("La solicitud tardó demasiado tiempo.");
        }
    }
}
