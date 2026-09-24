using System.Net.Http.Json;
using System.Text.Json;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Web.Services;

public sealed class UsuariosApiService
{
    private readonly HttpClient _httpClient;

    public UsuariosApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoApi<IReadOnlyList<UsuarioResponse>>> ObtenerAsync(
        string? buscar,
        CancellationToken cancellationToken = default)
    {
        var ruta = string.IsNullOrWhiteSpace(buscar)
            ? "api/usuarios"
            : $"api/usuarios?buscar={Uri.EscapeDataString(buscar.Trim())}";
        return await ObtenerListaAsync<UsuarioResponse>(ruta, "No fue posible obtener los usuarios.", cancellationToken);
    }

    public async Task<ResultadoApi<IReadOnlyList<RolOpcionResponse>>> ObtenerRolesDisponiblesAsync(
        CancellationToken cancellationToken = default) =>
        await ObtenerListaAsync<RolOpcionResponse>(
            "api/usuarios/roles-disponibles",
            "No fue posible obtener los roles disponibles.",
            cancellationToken);

    public Task<ResultadoApi<UsuarioResponse>> CrearAsync(
        CrearUsuarioRequest request,
        CancellationToken cancellationToken = default) =>
        EnviarAsync<UsuarioResponse>(HttpMethod.Post, "api/usuarios", request, "No fue posible crear el usuario.", cancellationToken);

    public Task<ResultadoApi<UsuarioResponse>> ActualizarAsync(
        int id,
        ActualizarUsuarioRequest request,
        CancellationToken cancellationToken = default) =>
        EnviarAsync<UsuarioResponse>(HttpMethod.Put, $"api/usuarios/{id}", request, "No fue posible actualizar el usuario.", cancellationToken);

    public async Task<ResultadoApi<bool>> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync($"api/usuarios/{id}", cancellationToken);
            if (response.IsSuccessStatusCode)
                return ResultadoApi<bool>.Correcto(true, "Usuario eliminado correctamente.", (int)response.StatusCode);

            var mensaje = await RespuestaApiReader.LeerErrorAsync(
                response, cancellationToken, "No fue posible eliminar el usuario.");
            return ResultadoApi<bool>.Fallo(mensaje, (int)response.StatusCode);
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

    private async Task<ResultadoApi<IReadOnlyList<T>>> ObtenerListaAsync<T>(
        string ruta,
        string mensajeError,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(ruta, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ResultadoApi<IReadOnlyList<T>>.Fallo(
                    await RespuestaApiReader.LeerErrorAsync(response, cancellationToken, mensajeError),
                    (int)response.StatusCode);

            var elementos = await response.Content.ReadFromJsonAsync<List<T>>(
                RespuestaApiReader.JsonOptions, cancellationToken) ?? [];
            return ResultadoApi<IReadOnlyList<T>>.Correcto(elementos, codigoHttp: (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ResultadoApi<IReadOnlyList<T>>.Fallo("No fue posible comunicarse con la API.");
        }
        catch (TaskCanceledException)
        {
            return ResultadoApi<IReadOnlyList<T>>.Fallo("La solicitud tardó demasiado tiempo.");
        }
        catch (JsonException)
        {
            return ResultadoApi<IReadOnlyList<T>>.Fallo("La API devolvió una respuesta inesperada.");
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
