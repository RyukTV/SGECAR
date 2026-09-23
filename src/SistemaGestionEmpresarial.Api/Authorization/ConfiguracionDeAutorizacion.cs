using Microsoft.AspNetCore.Authorization;
using SistemaGestionEmpresarial.Api.Services;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Authorization;

public static class ConfiguracionDeAutorizacion
{
    public static IServiceCollection AddAutorizacionPorPermisos(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioActual, UsuarioActual>();
        services.AddScoped<IAuthorizationHandler, PermisoAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, RespuestaDeAutorizacionHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var permiso in Permisos.Todos)
            {
                options.AddPolicy(PoliticasDePermiso.Nombre(permiso), policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new PermisoRequirement(permiso));
                });
            }

            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
