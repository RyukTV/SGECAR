using System.Security.Claims;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Services;

public sealed class UsuarioActual : IUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProveedorDePermisos _proveedorDePermisos;

    public UsuarioActual(IHttpContextAccessor httpContextAccessor, IProveedorDePermisos proveedorDePermisos)
    {
        _httpContextAccessor = httpContextAccessor;
        _proveedorDePermisos = proveedorDePermisos;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool EstaAutenticado => Principal?.Identity?.IsAuthenticated == true;

    public string? NombreUsuario => EstaAutenticado ? Principal!.Identity!.Name : null;

    public string? Rol => Principal.ObtenerRol();

    public Task<IReadOnlySet<string>> ObtenerPermisosAsync(CancellationToken cancellationToken = default) =>
        _proveedorDePermisos.ObtenerPermisosAsync(Rol, cancellationToken);

    public async Task<bool> TienePermisoAsync(string permiso, CancellationToken cancellationToken = default)
    {
        var permisos = await ObtenerPermisosAsync(cancellationToken);
        return permisos.Contains(permiso);
    }

    public async Task ExigirPermisoAsync(string permiso, CancellationToken cancellationToken = default)
    {
        if (!await TienePermisoAsync(permiso, cancellationToken))
        {
            throw new PermisoDenegadoException(Rol, permiso);
        }
    }
}
