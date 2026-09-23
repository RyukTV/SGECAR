using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Authorization;

/// <summary>
/// Traduce a un 403 con cuerpo JSON las <see cref="PermisoDenegadoException"/> que lanzan los
/// Services, para que una comprobación hecha en la capa de negocio produzca la misma respuesta
/// que una hecha con <c>[Permiso]</c> en el Controller.
/// </summary>
public sealed class MiddlewareDePermisoDenegado
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MiddlewareDePermisoDenegado> _logger;

    public MiddlewareDePermisoDenegado(RequestDelegate next, ILogger<MiddlewareDePermisoDenegado> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (PermisoDenegadoException ex)
        {
            _logger.LogWarning(
                "Acceso denegado en la capa de servicios: rol '{Rol}', permiso '{Permiso}'.",
                ex.Rol,
                ex.PermisoRequerido);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(
                AccesoDenegadoResponse.PermisosInsuficientes(ex.Rol, ex.PermisoRequerido));
        }
    }
}
