using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public interface ICategoriasProductoService
{
    Task<IReadOnlyList<CategoriaProductoResponse>> ObtenerTodasAsync(
        string? buscar,
        CancellationToken cancellationToken = default);
    Task<CategoriaProductoResponse?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ResultadoCrud<CategoriaProductoResponse>> CrearAsync(
        CrearCategoriaProductoRequest request,
        CancellationToken cancellationToken = default);
    Task<ResultadoCrud<CategoriaProductoResponse>> ActualizarAsync(
        int id,
        ActualizarCategoriaProductoRequest request,
        CancellationToken cancellationToken = default);
    Task<ResultadoCrud<bool>> EliminarAsync(int id, CancellationToken cancellationToken = default);
}
