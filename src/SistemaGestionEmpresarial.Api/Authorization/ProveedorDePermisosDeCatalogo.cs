using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Authorization;

/// <summary>
/// Implementación vigente en la Etapa I: los permisos salen del catálogo compartido, que
/// reproduce el seed de roles. No consulta la base de datos, de modo que la autorización
/// funciona igual aunque la tabla <c>Roles</c> todavía esté evolucionando.
/// </summary>
public sealed class ProveedorDePermisosDeCatalogo : IProveedorDePermisos
{
    public Task<IReadOnlySet<string>> ObtenerPermisosAsync(string? rol, CancellationToken cancellationToken = default) =>
        Task.FromResult(CatalogoDePermisos.ObtenerPermisos(rol));
}
