using Microsoft.AspNetCore.Authorization;
using SistemaGestionEmpresarial.Api.Services;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Authorization;

/// <summary>Registro del control de acceso por permisos de la API.</summary>
public static class ConfiguracionDeAutorizacion
{
    public static IServiceCollection AddAutorizacionPorPermisos(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IProveedorDePermisos, ProveedorDePermisosDeCatalogo>();
        services.AddScoped<IUsuarioActual, UsuarioActual>();
        services.AddScoped<IAuthorizationHandler, PermisoAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, RespuestaDeAutorizacionHandler>();

        services.AddAuthorization(options =>
        {
            // Una política por permiso: "Permiso:Consultar", "Permiso:Eliminar", etc.
            foreach (var permiso in Permisos.Todos)
            {
                options.AddPolicy(PoliticasDePermiso.Nombre(permiso), policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new PermisoRequirement(permiso));
                });
            }

            // Cerrado por omisión: un endpoint nuevo exige sesión aunque se olvide anotarlo.
            // Los públicos (login, health) deben marcarse con [AllowAnonymous].
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
