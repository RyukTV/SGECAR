using System.Net.Http.Json;
using System.Text.Json;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Web.Services;

public sealed class ProductosApiService
{
    private readonly HttpClient _httpClient;

    public ProductosApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoApi<IReadOnlyList<ProductoResponse>>> ObtenerAsync(
        string? codigoOId,
        string? nombre,
        int? categoriaId,
        CancellationToken cancellationToken = default)
    {
        var parametros = new List<string>();
        if (!string.IsNullOrWhiteSpace(codigoOId))
            parametros.Add($"codigoOId={Uri.EscapeDataString(codigoOId.Trim())}");
        if (!string.IsNullOrWhiteSpace(nombre))
            parametros.Add($"nombre={Uri.EscapeDataString(nombre.Trim())}");
        if (categoriaId.HasValue)
            parametros.Add($"categoriaId={categoriaId.Value}");

        var ruta = parametros.Count == 0 ? "api/productos" : $"api/productos?{string.Join("&", parametros)}";

        try
        {
            using var response = await _httpClient.GetAsync(ruta, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ResultadoApi<IReadOnlyList<ProductoResponse>>.Fallo(
                    await RespuestaApiReader.LeerErrorAsync(
                        response, cancellationToken, "No fue posible obtener los productos."),
                    (int)response.StatusCode);

            var productos = await response.Content.ReadFromJsonAsync<List<ProductoResponse>>(
                RespuestaApiReader.JsonOptions, cancellationToken) ?? [];
            return ResultadoApi<IReadOnlyList<ProductoResponse>>.Correcto(
                productos, codigoHttp: (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ResultadoApi<IReadOnlyList<ProductoResponse>>.Fallo("No fue posible comunicarse con la API.");
        }
        catch (TaskCanceledException)
        {
            return ResultadoApi<IReadOnlyList<ProductoResponse>>.Fallo("La solicitud tardó demasiado tiempo.");
        }
        catch (JsonException)
        {
            return ResultadoApi<IReadOnlyList<ProductoResponse>>.Fallo("La API devolvió una respuesta inesperada.");
        }
    }

    public async Task<ResultadoApi<IReadOnlyList<CategoriaProductoResponse>>> ObtenerCategoriasDisponiblesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(
                "api/productos/categorias-disponibles", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ResultadoApi<IReadOnlyList<CategoriaProductoResponse>>.Fallo(
                    await RespuestaApiReader.LeerErrorAsync(
                        response, cancellationToken, "No fue posible obtener las categorías."),
                    (int)response.StatusCode);

            var categorias = await response.Content.ReadFromJsonAsync<List<CategoriaProductoResponse>>(
                RespuestaApiReader.JsonOptions, cancellationToken) ?? [];
            return ResultadoApi<IReadOnlyList<CategoriaProductoResponse>>.Correcto(
                categorias, codigoHttp: (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ResultadoApi<IReadOnlyList<CategoriaProductoResponse>>.Fallo("No fue posible comunicarse con la API.");
        }
        catch (TaskCanceledException)
        {
            return ResultadoApi<IReadOnlyList<CategoriaProductoResponse>>.Fallo("La solicitud tardó demasiado tiempo.");
        }
        catch (JsonException)
        {
            return ResultadoApi<IReadOnlyList<CategoriaProductoResponse>>.Fallo("La API devolvió una respuesta inesperada.");
        }
    }

    public Task<ResultadoApi<ProductoResponse>> CrearAsync(
        CrearProductoRequest request,
        CancellationToken cancellationToken = default) =>
        EnviarAsync<ProductoResponse>(
            HttpMethod.Post, "api/productos", request, "No fue posible crear el producto.", cancellationToken);

    public Task<ResultadoApi<ProductoResponse>> ActualizarAsync(
        int id,
        ActualizarProductoRequest request,
        CancellationToken cancellationToken = default) =>
        EnviarAsync<ProductoResponse>(
            HttpMethod.Put, $"api/productos/{id}", request, "No fue posible actualizar el producto.", cancellationToken);

    public async Task<ResultadoApi<bool>> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync($"api/productos/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
                return ResultadoApi<bool>.Correcto(true, "Producto eliminado correctamente.", (int)response.StatusCode);

            return ResultadoApi<bool>.Fallo(
                await RespuestaApiReader.LeerErrorAsync(
                    response, cancellationToken, "No fue posible eliminar el producto."),
                (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ResultadoApi<bool>.Fallo("No fue posible comunicarse con la API.");
        }
        catch (TaskCanceledException)
        {
            return ResultadoApi<bool>.Fallo("La solicitud tardó demasiado tiempo.");
        }
    }

    private async Task<ResultadoApi<T>> EnviarAsync<T>(
        HttpMethod metodo,
        string ruta,
        object contenido,
        string mensajeError,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(metodo, ruta)
            {
                Content = JsonContent.Create(contenido)
            };
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ResultadoApi<T>.Fallo(
                    await RespuestaApiReader.LeerErrorAsync(response, cancellationToken, mensajeError),
                    (int)response.StatusCode);

            var valor = await response.Content.ReadFromJsonAsync<T>(
                RespuestaApiReader.JsonOptions, cancellationToken);
            return valor is null
                ? ResultadoApi<T>.Fallo("La API devolvió una respuesta inesperada.", (int)response.StatusCode)
                : ResultadoApi<T>.Correcto(valor, codigoHttp: (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ResultadoApi<T>.Fallo("No fue posible comunicarse con la API.");
        }
        catch (TaskCanceledException)
        {
            return ResultadoApi<T>.Fallo("La solicitud tardó demasiado tiempo.");
        }
        catch (JsonException)
        {
            return ResultadoApi<T>.Fallo("La API devolvió una respuesta inesperada.");
        }
    }
}
