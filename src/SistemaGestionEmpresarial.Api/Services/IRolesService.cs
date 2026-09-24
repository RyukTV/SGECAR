using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public interface IRolesService
{
    Task<IReadOnlyList<RolResponse>> ObtenerTodosAsync(string? buscar, CancellationToken cancellationToken = default);
    Task<RolResponse?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ResultadoCrud<RolResponse>> CrearAsync(CrearRolRequest request, CancellationToken cancellationToken = default);
    Task<ResultadoCrud<RolResponse>> ActualizarAsync(int id, ActualizarRolRequest request, CancellationToken cancellationToken = default);
    Task<ResultadoCrud<bool>> EliminarAsync(int id, CancellationToken cancellationToken = default);
}
