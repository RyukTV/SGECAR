using Microsoft.Extensions.DependencyInjection;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Web.Authorization;

/// <summary>
/// Registra en Blazor las mismas políticas que la API, evaluadas sobre el rol del JWT.
/// Gracias a esto <c>AuthorizeView</c>, <c>[Authorize(Policy = ...)]</c> y el filtro de rutas
/// usan exactamente la misma regla que el servidor.
/// </summary>
public static class ConfiguracionDeAutorizacion
{
    public static IServiceCollection AddAutorizacionPorPermisos(this IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            foreach (var permiso in Permisos.Todos)
            {
                var permisoExigido = permiso;

                options.AddPolicy(
                    PoliticasDePermiso.Nombre(permisoExigido),
                    policy => policy.RequireAssertion(contexto => contexto.User.TienePermiso(permisoExigido)));
            }
        });

        return services;
    }
}
