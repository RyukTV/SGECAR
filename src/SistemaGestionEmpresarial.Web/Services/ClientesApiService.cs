using System.Net.Http.Json;
using System.Text.Json;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Web.Services;

public sealed class ClientesApiService
{
    private readonly HttpClient _httpClient;

    public ClientesApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoApi<IReadOnlyList<ClienteResponse>>> ObtenerAsync(
        int? id,
        string? buscar,
        string? correo,
        CancellationToken cancellationToken = default)
    {
        var parametros = new List<string>();
        if (id.HasValue) parametros.Add($"id={id.Value}");
        if (!string.IsNullOrWhiteSpace(buscar))
            parametros.Add($"buscar={Uri.EscapeDataString(buscar.Trim())}");
        if (!string.IsNullOrWhiteSpace(correo))
            parametros.Add($"correo={Uri.EscapeDataString(correo.Trim())}");

        var ruta = parametros.Count == 0 ? "api/clientes" : $"api/clientes?{string.Join("&", parametros)}";

        try
        {
            using var response = await _httpClient.GetAsync(ruta, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ResultadoApi<IReadOnlyList<ClienteResponse>>.Fallo(
                    await RespuestaApiReader.LeerErrorAsync(
                        response, cancellationToken, "No fue posible obtener los clientes."),
                    (int)response.StatusCode);

            var clientes = await response.Content.ReadFromJsonAsync<List<ClienteResponse>>(
                RespuestaApiReader.JsonOptions, cancellationToken) ?? [];
            return ResultadoApi<IReadOnlyList<ClienteResponse>>.Correcto(
                clientes, codigoHttp: (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ResultadoApi<IReadOnlyList<ClienteResponse>>.Fallo("No fue posible comunicarse con la API.");
        }
        catch (TaskCanceledException)
        {
            return ResultadoApi<IReadOnlyList<ClienteResponse>>.Fallo("La solicitud tardó demasiado tiempo.");
        }
        catch (JsonException)
        {
            return ResultadoApi<IReadOnlyList<ClienteResponse>>.Fallo("La API devolvió una respuesta inesperada.");
        }
    }

    public Task<ResultadoApi<ClienteResponse>> CrearAsync(
        CrearClienteRequest request,
        CancellationToken cancellationToken = default) =>
        EnviarAsync<ClienteResponse>(
            HttpMethod.Post, "api/clientes", request, "No fue posible crear el cliente.", cancellationToken);

    public Task<ResultadoApi<ClienteResponse>> ActualizarAsync(
        int id,
        ActualizarClienteRequest request,
        CancellationToken cancellationToken = default) =>
        EnviarAsync<ClienteResponse>(
            HttpMethod.Put, $"api/clientes/{id}", request, "No fue posible actualizar el cliente.", cancellationToken);

    public async Task<ResultadoApi<bool>> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync($"api/clientes/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
                return ResultadoApi<bool>.Correcto(true, "Cliente eliminado correctamente.", (int)response.StatusCode);

            return ResultadoApi<bool>.Fallo(
                await RespuestaApiReader.LeerErrorAsync(
                    response, cancellationToken, "No fue posible eliminar el cliente."),
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
