using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public interface IClientesService
{
    Task<IReadOnlyList<ClienteResponse>> ObtenerTodosAsync(
        int? id,
        string? buscar,
        string? correo,
        CancellationToken cancellationToken = default);

    Task<ClienteResponse?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ResultadoCrud<ClienteResponse>> CrearAsync(CrearClienteRequest request, CancellationToken cancellationToken = default);
    Task<ResultadoCrud<ClienteResponse>> ActualizarAsync(int id, ActualizarClienteRequest request, CancellationToken cancellationToken = default);
    Task<ResultadoCrud<bool>> EliminarAsync(int id, CancellationToken cancellationToken = default);
}
