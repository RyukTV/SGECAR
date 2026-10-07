using System.Security.Claims;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Services;

public sealed class UsuarioActual : IUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioActual(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool EstaAutenticado => Principal?.Identity?.IsAuthenticated == true;

    public int? Id => EstaAutenticado &&
                      int.TryParse(Principal!.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : null;

    public string? NombreUsuario => EstaAutenticado ? Principal!.Identity!.Name : null;

    public string? Rol => Principal.ObtenerRol();

    public Task<IReadOnlySet<string>> ObtenerPermisosAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Principal.ObtenerPermisos());

    public Task<bool> TienePermisoAsync(string permiso, CancellationToken cancellationToken = default) =>
        Task.FromResult(Principal.TienePermiso(permiso));

    public async Task ExigirPermisoAsync(string permiso, CancellationToken cancellationToken = default)
    {
        if (!await TienePermisoAsync(permiso, cancellationToken))
        {
            throw new PermisoDenegadoException(Rol, permiso);
        }
    }
}
