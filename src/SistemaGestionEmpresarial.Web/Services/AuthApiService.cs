using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Web.Services;

public sealed class AuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authStateProvider;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthApiService(HttpClient httpClient, CustomAuthStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Envía las credenciales al backend API y actualiza el estado de autenticación en caso de éxito.
    /// </summary>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return LoginResponse.Fail("Debe ingresar tanto el usuario como la contraseña.");
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var okResult = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
                if (okResult?.Success == true && !string.IsNullOrWhiteSpace(okResult.Token))
                {
                    await _authStateProvider.SetAuthenticatedAsync(okResult.Token);
                    return okResult;
                }

                return okResult ?? LoginResponse.Fail("Respuesta inesperada del servidor.");
            }

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var errorResult = JsonSerializer.Deserialize<LoginResponse>(content, JsonOptions);
                if (errorResult != null && !string.IsNullOrWhiteSpace(errorResult.Message))
                {
                    return errorResult;
                }
            }
            catch
            {
                // Si el formato no es LoginResponse
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return LoginResponse.Fail("Usuario o contraseña incorrectos.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return LoginResponse.Fail("Los campos enviados son inválidos o están incompletos.");
            }

            return LoginResponse.Fail($"Error al comunicarse con el servidor ({response.StatusCode}).");
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error de conexión con la API: {ex}");
            return LoginResponse.Fail("No fue posible comunicarse con el servicio de autenticación.");
        }
        catch (TaskCanceledException)
        {
            return LoginResponse.Fail("La solicitud al servidor tardó demasiado tiempo (Timeout).");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inesperado durante la autenticación: {ex}");
            return LoginResponse.Fail("Ocurrió un error inesperado durante la autenticación.");
        }
    }

    /// <summary>
    /// Cierra la sesión activa y limpia los datos de autenticación en Blazor.
    /// </summary>
    public async Task LogoutAsync()
    {
        await _authStateProvider.SetLoggedOutAsync();
    }
}
