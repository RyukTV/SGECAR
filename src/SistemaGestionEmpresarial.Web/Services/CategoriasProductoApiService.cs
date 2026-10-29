using System.Net.Http.Json;
using System.Text.Json;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Web.Services;

public sealed class CategoriasProductoApiService
{
    private readonly HttpClient _httpClient;

    public CategoriasProductoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoApi<IReadOnlyList<CategoriaProductoResponse>>> ObtenerAsync(
        string? buscar,
        CancellationToken cancellationToken = default)
    {
        var ruta = string.IsNullOrWhiteSpace(buscar)
            ? "api/categorias-productos"
            : $"api/categorias-productos?buscar={Uri.EscapeDataString(buscar.Trim())}";
        try
        {
            using var response = await _httpClient.GetAsync(ruta, cancellationToken);
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

    public Task<ResultadoApi<CategoriaProductoResponse>> CrearAsync(
        CrearCategoriaProductoRequest request,
        CancellationToken cancellationToken = default) =>
        EnviarAsync<CategoriaProductoResponse>(
            HttpMethod.Post, "api/categorias-productos", request,
            "No fue posible crear la categoría.", cancellationToken);

    public Task<ResultadoApi<CategoriaProductoResponse>> ActualizarAsync(
        int id,
        ActualizarCategoriaProductoRequest request,
        CancellationToken cancellationToken = default) =>
        EnviarAsync<CategoriaProductoResponse>(
            HttpMethod.Put, $"api/categorias-productos/{id}", request,
            "No fue posible actualizar la categoría.", cancellationToken);

    public async Task<ResultadoApi<bool>> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync($"api/categorias-productos/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
                return ResultadoApi<bool>.Correcto(
                    true, "Categoría eliminada correctamente.", (int)response.StatusCode);

            return ResultadoApi<bool>.Fallo(
                await RespuestaApiReader.LeerErrorAsync(
                    response, cancellationToken, "No fue posible eliminar la categoría."),
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
