using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public interface IProductosService
{
    Task<IReadOnlyList<ProductoResponse>> ObtenerTodosAsync(
        int? id,
        string? buscar,
        string? categoria,
        CancellationToken cancellationToken = default);

    Task<ProductoResponse?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ObtenerCategoriasAsync(
        CancellationToken cancellationToken = default);

    Task<ResultadoCrud<ProductoResponse>> CrearAsync(
        CrearProductoRequest request,
        CancellationToken cancellationToken = default);

    Task<ResultadoCrud<ProductoResponse>> ActualizarAsync(
        int id,
        ActualizarProductoRequest request,
        CancellationToken cancellationToken = default);

    Task<ResultadoCrud<bool>> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default);
}
