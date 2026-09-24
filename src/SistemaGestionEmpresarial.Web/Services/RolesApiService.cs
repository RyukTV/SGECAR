using System.Net.Http.Json;
using System.Text.Json;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Web.Services;

public sealed class RolesApiService
{
    private readonly HttpClient _httpClient;

    public RolesApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoApi<IReadOnlyList<RolResponse>>> ObtenerAsync(
        string? buscar,
        CancellationToken cancellationToken = default)
    {
        var ruta = string.IsNullOrWhiteSpace(buscar)
            ? "api/roles"
            : $"api/roles?buscar={Uri.EscapeDataString(buscar.Trim())}";

        try
        {
            using var response = await _httpClient.GetAsync(ruta, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ResultadoApi<IReadOnlyList<RolResponse>>.Fallo(
                    await RespuestaApiReader.LeerErrorAsync(response, cancellationToken, "No fue posible obtener los roles."),
                    (int)response.StatusCode);

            var roles = await response.Content.ReadFromJsonAsync<List<RolResponse>>(
                RespuestaApiReader.JsonOptions, cancellationToken) ?? [];
            return ResultadoApi<IReadOnlyList<RolResponse>>.Correcto(roles, codigoHttp: (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ResultadoApi<IReadOnlyList<RolResponse>>.Fallo("No fue posible comunicarse con la API.");
        }
        catch (TaskCanceledException)
        {
            return ResultadoApi<IReadOnlyList<RolResponse>>.Fallo("La solicitud tardó demasiado tiempo.");
        }
        catch (JsonException)
        {
            return ResultadoApi<IReadOnlyList<RolResponse>>.Fallo("La API devolvió una respuesta inesperada.");
        }
    }

    public Task<ResultadoApi<RolResponse>> CrearAsync(
        CrearRolRequest request,
        CancellationToken cancellationToken = default) =>
        EnviarAsync<RolResponse>(HttpMethod.Post, "api/roles", request, "No fue posible crear el rol.", cancellationToken);

    public Task<ResultadoApi<RolResponse>> ActualizarAsync(
        int id,
        ActualizarRolRequest request,
        CancellationToken cancellationToken = default) =>
        EnviarAsync<RolResponse>(HttpMethod.Put, $"api/roles/{id}", request, "No fue posible actualizar el rol.", cancellationToken);

    public async Task<ResultadoApi<bool>> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync($"api/roles/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
                return ResultadoApi<bool>.Correcto(true, "Rol eliminado correctamente.", (int)response.StatusCode);

            return ResultadoApi<bool>.Fallo(
                await RespuestaApiReader.LeerErrorAsync(response, cancellationToken, "No fue posible eliminar el rol."),
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
