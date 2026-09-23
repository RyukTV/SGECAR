using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Authorization;

/// <summary>
/// Sustituye los 401 y 403 con cuerpo vacío de ASP.NET Core por un JSON que el cliente Blazor
/// puede mostrar tal cual al usuario, indicando qué permiso faltaba.
/// </summary>
public sealed class RespuestaDeAutorizacionHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _handlerPredeterminado = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            await EscribirRespuestaAsync(context, StatusCodes.Status401Unauthorized, AccesoDenegadoResponse.NoAutenticado());
            return;
        }

        if (authorizeResult.Forbidden)
        {
            var permisoRequerido = authorizeResult.AuthorizationFailure?.FailedRequirements
                .OfType<PermisoRequirement>()
                .Select(requisito => requisito.Permiso)
                .FirstOrDefault();

            var respuesta = AccesoDenegadoResponse.PermisosInsuficientes(context.User.ObtenerRol(), permisoRequerido);
            await EscribirRespuestaAsync(context, StatusCodes.Status403Forbidden, respuesta);
            return;
        }

        await _handlerPredeterminado.HandleAsync(next, context, policy, authorizeResult);
    }

    private static async Task EscribirRespuestaAsync(HttpContext context, int statusCode, AccesoDenegadoResponse respuesta)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(respuesta);
    }
}
