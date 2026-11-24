using System.Net.Http.Json;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Web.Services;

public sealed class ApiClient(HttpClient httpClient)
{
    public async Task<HealthResponse> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<HealthResponse>("api/health", cancellationToken)
            ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
    }
}
