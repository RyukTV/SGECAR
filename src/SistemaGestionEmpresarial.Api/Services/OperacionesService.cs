using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Services;

public sealed class OperacionesService : IOperacionesService
{
    private readonly IUsuarioActual _usuarioActual;
    private readonly ILogger<OperacionesService> _logger;

    public OperacionesService(IUsuarioActual usuarioActual, ILogger<OperacionesService> logger)
    {
        _usuarioActual = usuarioActual;
        _logger = logger;
    }

    public async Task<OperacionResponse> EjecutarAsync(string permiso, CancellationToken cancellationToken = default)
    {
        // Segunda barrera. El Controller ya exigió el permiso con [Permiso], pero el servicio
        // vuelve a comprobarlo para que ninguna vía de llamada futura quede sin control.
        await _usuarioActual.ExigirPermisoAsync(permiso, cancellationToken);

        _logger.LogInformation(
            "Operación '{Permiso}' autorizada para el usuario '{Usuario}' con rol '{Rol}'.",
            permiso,
            _usuarioActual.NombreUsuario,
            _usuarioActual.Rol);

        return OperacionResponse.Realizada(permiso, _usuarioActual.Rol);
    }
}
