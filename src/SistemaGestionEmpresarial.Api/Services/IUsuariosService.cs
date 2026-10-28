using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public interface IUsuariosService
{
    Task<IReadOnlyList<UsuarioResponse>> ObtenerTodosAsync(
        int? id,
        string? buscar,
        int? rolId,
        bool? activo,
        CancellationToken cancellationToken = default);

    Task<UsuarioResponse?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolOpcionResponse>> ObtenerRolesDisponiblesAsync(CancellationToken cancellationToken = default);
    Task<ResultadoCrud<UsuarioResponse>> CrearAsync(CrearUsuarioRequest request, CancellationToken cancellationToken = default);
    Task<ResultadoCrud<UsuarioResponse>> ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken cancellationToken = default);
    Task<ResultadoCrud<bool>> EliminarAsync(int id, CancellationToken cancellationToken = default);
}
