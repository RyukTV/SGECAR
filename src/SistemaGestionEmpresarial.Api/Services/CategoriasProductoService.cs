using Microsoft.EntityFrameworkCore;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Api.Entities;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public sealed class CategoriasProductoService : ICategoriasProductoService
{
    private const string MensajeCategoriaEnUso =
        "No se puede eliminar la categoría porque tiene productos asociados.";

    private readonly AppDbContext _dbContext;

    public CategoriasProductoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoriaProductoResponse>> ObtenerTodasAsync(
        string? buscar,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CategoriasProducto.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            query = query.Where(categoria => categoria.Nombre.Contains(termino));
        }

        return await query
            .OrderBy(categoria => categoria.Nombre)
            .Select(categoria => Mapear(categoria))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoriaProductoResponse?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        await _dbContext.CategoriasProducto
            .AsNoTracking()
            .Where(categoria => categoria.Id == id)
            .Select(categoria => Mapear(categoria))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ResultadoCrud<CategoriaProductoResponse>> CrearAsync(
        CrearCategoriaProductoRequest request,
        CancellationToken cancellationToken = default)
    {
        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return ResultadoCrud<CategoriaProductoResponse>.Validacion("El nombre de la categoría es obligatorio.");

        if (await ExisteNombreAsync(nombre, null, cancellationToken))
            return ResultadoCrud<CategoriaProductoResponse>.Conflicto("Ya existe una categoría con ese nombre.");

        var categoria = new CategoriaProducto { Nombre = nombre };
        _dbContext.CategoriasProducto.Add(categoria);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ResultadoCrud<CategoriaProductoResponse>.Conflicto("Ya existe una categoría con ese nombre.");
        }

        return ResultadoCrud<CategoriaProductoResponse>.Exito(
            Mapear(categoria), "Categoría creada correctamente.");
    }

    public async Task<ResultadoCrud<CategoriaProductoResponse>> ActualizarAsync(
        int id,
        ActualizarCategoriaProductoRequest request,
        CancellationToken cancellationToken = default)
    {
        var categoria = await _dbContext.CategoriasProducto.SingleOrDefaultAsync(
            entidad => entidad.Id == id, cancellationToken);
        if (categoria is null)
            return ResultadoCrud<CategoriaProductoResponse>.NoEncontrado("La categoría solicitada no existe.");

        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return ResultadoCrud<CategoriaProductoResponse>.Validacion("El nombre de la categoría es obligatorio.");

        if (await ExisteNombreAsync(nombre, id, cancellationToken))
            return ResultadoCrud<CategoriaProductoResponse>.Conflicto("Ya existe una categoría con ese nombre.");

        categoria.Nombre = nombre;
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ResultadoCrud<CategoriaProductoResponse>.Conflicto("Ya existe una categoría con ese nombre.");
        }

        return ResultadoCrud<CategoriaProductoResponse>.Exito(
            Mapear(categoria), "Categoría actualizada correctamente.");
    }

    public async Task<ResultadoCrud<bool>> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var categoria = await _dbContext.CategoriasProducto.SingleOrDefaultAsync(
            entidad => entidad.Id == id, cancellationToken);
        if (categoria is null)
            return ResultadoCrud<bool>.NoEncontrado("La categoría solicitada no existe.");

        if (await _dbContext.Productos.AnyAsync(producto => producto.CategoriaId == id, cancellationToken))
            return ResultadoCrud<bool>.Conflicto(MensajeCategoriaEnUso);

        _dbContext.CategoriasProducto.Remove(categoria);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ResultadoCrud<bool>.Conflicto(MensajeCategoriaEnUso);
        }

        return ResultadoCrud<bool>.Exito(true, "Categoría eliminada correctamente.");
    }

    private Task<bool> ExisteNombreAsync(string nombre, int? idExcluido, CancellationToken cancellationToken) =>
        _dbContext.CategoriasProducto.AnyAsync(
            categoria => categoria.Nombre == nombre && (!idExcluido.HasValue || categoria.Id != idExcluido.Value),
            cancellationToken);

    private static CategoriaProductoResponse Mapear(CategoriaProducto categoria) => new()
    {
        Id = categoria.Id,
        Nombre = categoria.Nombre
    };
}
