using System.Text.Json;

namespace SistemaGestionEmpresarial.Web.Services;

internal static class RespuestaApiReader
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<string> LeerErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken,
        string mensajePredeterminado)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = document.RootElement;

            if (root.TryGetProperty("message", out var message) && !string.IsNullOrWhiteSpace(message.GetString()))
                return message.GetString()!;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var propiedad in errors.EnumerateObject())
                {
                    if (propiedad.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var error in propiedad.Value.EnumerateArray())
                        {
                            var mensaje = error.GetString();
                            if (!string.IsNullOrWhiteSpace(mensaje))
                                return mensaje;
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // La interfaz usa el mensaje general y nunca expone el contenido técnico recibido.
        }

        return mensajePredeterminado;
    }
}
