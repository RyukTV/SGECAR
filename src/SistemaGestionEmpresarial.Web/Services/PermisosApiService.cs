using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Web.Services;

/// <summary>
/// Cliente de los endpoints de permisos y operaciones. Centraliza la traducción de los 401 y
/// 403 de la API a mensajes que la interfaz puede mostrar sin inventarse el texto.
/// </summary>
public sealed class PermisosApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public PermisosApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>Permisos que la API reconoce al usuario autenticado.</summary>
    public async Task<PermisosResponse?> ObtenerMisPermisosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/permisos/mios", cancellationToken);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<PermisosResponse>(JsonOptions, cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return null;
        }
    }

    /// <summary>Matriz completa rol → permisos. La API solo la entrega a quien gestiona roles.</summary>
    public async Task<IReadOnlyList<RolPermisosResponse>?> ObtenerCatalogoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/permisos/catalogo", cancellationToken);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<IReadOnlyList<RolPermisosResponse>>(JsonOptions, cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Invoca en la API la operación correspondiente al permiso indicado. Se llama siempre,
    /// tenga o no el usuario el permiso en la interfaz: es el servidor quien decide.
    /// </summary>
    public async Task<ResultadoDeOperacion> EjecutarOperacionAsync(string permiso, CancellationToken cancellationToken = default)
    {
        var solicitud = ConstruirSolicitud(permiso);

        if (solicitud is null)
        {
            return ResultadoDeOperacion.Fallo($"La operación '{permiso}' no tiene un endpoint asociado.");
        }

        try
        {
            using var request = solicitud;
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var codigo = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var exito = await response.Content.ReadFromJsonAsync<OperacionResponse>(JsonOptions, cancellationToken);
                return ResultadoDeOperacion.Permitida(
                    exito?.Message ?? "Operación realizada correctamente.",
                    codigo);
            }

            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            {
                var denegado = await LeerAccesoDenegadoAsync(response, cancellationToken);
                return ResultadoDeOperacion.Rechazada(
                    denegado?.Message ?? "El servidor rechazó la operación por falta de permisos.",
                    codigo);
            }

            return ResultadoDeOperacion.Fallo($"Error al comunicarse con el servidor ({response.StatusCode}).", codigo);
        }
        catch (HttpRequestException)
        {
            return ResultadoDeOperacion.Fallo("No fue posible comunicarse con la API.");
        }
        catch (TaskCanceledException)
        {
            return ResultadoDeOperacion.Fallo("La solicitud al servidor tardó demasiado tiempo.");
        }
        catch (JsonException)
        {
            return ResultadoDeOperacion.Fallo("La API devolvió una respuesta con un formato inesperado.");
        }
    }

    private static HttpRequestMessage? ConstruirSolicitud(string permiso) => permiso switch
    {
        Permisos.Consultar => new HttpRequestMessage(HttpMethod.Get, "api/operaciones/consultar"),
        Permisos.Agregar => new HttpRequestMessage(HttpMethod.Post, "api/operaciones/agregar"),
        Permisos.Modificar => new HttpRequestMessage(HttpMethod.Put, "api/operaciones/modificar"),
        Permisos.Eliminar => new HttpRequestMessage(HttpMethod.Delete, "api/operaciones/eliminar"),
        _ => null
    };

    private static async Task<AccesoDenegadoResponse?> LeerAccesoDenegadoAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<AccesoDenegadoResponse>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
