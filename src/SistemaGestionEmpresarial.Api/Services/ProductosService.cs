using Microsoft.EntityFrameworkCore;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Api.Entities;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public sealed class ProductosService : IProductosService
{
    private readonly AppDbContext _dbContext;

    public ProductosService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductoResponse>> ObtenerTodosAsync(
        int? id,
        string? buscar,
        string? categoria,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Productos.AsNoTracking().AsQueryable();

        if (id.HasValue)
        {
            query = query.Where(producto => producto.Id == id.Value);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            query = query.Where(producto =>
                producto.Nombre.Contains(termino) ||
                producto.Codigo.Contains(termino));
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var categoriaFiltro = categoria.Trim();
            query = query.Where(producto => producto.Categoria == categoriaFiltro);
        }

        return await query
            .OrderBy(producto => producto.Nombre)
            .Select(producto => Mapear(producto))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductoResponse?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Productos
            .AsNoTracking()
            .Where(producto => producto.Id == id)
            .Select(producto => Mapear(producto))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> ObtenerCategoriasAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.Productos
            .AsNoTracking()
            .Select(producto => producto.Categoria)
            .Where(cat => !string.IsNullOrWhiteSpace(cat))
            .Distinct()
            .OrderBy(cat => cat)
            .ToListAsync(cancellationToken);

    public async Task<ResultadoCrud<ProductoResponse>> CrearAsync(
        CrearProductoRequest request,
        CancellationToken cancellationToken = default)
    {
        var datos = Normalizar(request.Codigo, request.Nombre, request.Precio, request.Stock, request.Categoria);
        var error = Validar(datos);
        if (error is not null)
        {
            return ResultadoCrud<ProductoResponse>.Validacion(error);
        }

        if (await _dbContext.Productos.AnyAsync(p => p.Codigo == datos.Codigo, cancellationToken))
        {
            return ResultadoCrud<ProductoResponse>.Conflicto("Ya existe un producto con ese código.");
        }

        var producto = new Producto
        {
            Codigo = datos.Codigo,
            Nombre = datos.Nombre,
            Precio = datos.Precio,
            Stock = datos.Stock,
            Categoria = datos.Categoria
        };

        _dbContext.Productos.Add(producto);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ResultadoCrud<ProductoResponse>.Conflicto("Ya existe un producto con ese código.");
        }

        return ResultadoCrud<ProductoResponse>.Exito(Mapear(producto), "Producto creado correctamente.");
    }

    public async Task<ResultadoCrud<ProductoResponse>> ActualizarAsync(
        int id,
        ActualizarProductoRequest request,
        CancellationToken cancellationToken = default)
    {
        var producto = await _dbContext.Productos.SingleOrDefaultAsync(
            p => p.Id == id,
            cancellationToken);

        if (producto is null)
        {
            return ResultadoCrud<ProductoResponse>.NoEncontrado("El producto solicitado no existe.");
        }

        var datos = Normalizar(request.Codigo, request.Nombre, request.Precio, request.Stock, request.Categoria);
        var error = Validar(datos);
        if (error is not null)
        {
            return ResultadoCrud<ProductoResponse>.Validacion(error);
        }

        if (await _dbContext.Productos.AnyAsync(
                p => p.Id != id && p.Codigo == datos.Codigo,
                cancellationToken))
        {
            return ResultadoCrud<ProductoResponse>.Conflicto("Ya existe un producto con ese código.");
        }

        producto.Codigo = datos.Codigo;
        producto.Nombre = datos.Nombre;
        producto.Precio = datos.Precio;
        producto.Stock = datos.Stock;
        producto.Categoria = datos.Categoria;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ResultadoCrud<ProductoResponse>.Conflicto("Ya existe un producto con ese código.");
        }

        return ResultadoCrud<ProductoResponse>.Exito(Mapear(producto), "Producto actualizado correctamente.");
    }

    public async Task<ResultadoCrud<bool>> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var producto = await _dbContext.Productos.SingleOrDefaultAsync(
            p => p.Id == id,
            cancellationToken);

        if (producto is null)
        {
            return ResultadoCrud<bool>.NoEncontrado("El producto solicitado no existe.");
        }

        _dbContext.Productos.Remove(producto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ResultadoCrud<bool>.Exito(true, "Producto eliminado correctamente.");
    }

    private static DatosProducto Normalizar(
        string codigo,
        string nombre,
        decimal precio,
        int stock,
        string categoria) =>
        new(codigo.Trim().ToUpperInvariant(), nombre.Trim(), precio, stock, categoria.Trim());

    private static string? Validar(DatosProducto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Codigo))
            return "El código es obligatorio.";

        if (string.IsNullOrWhiteSpace(datos.Nombre))
            return "El nombre es obligatorio.";

        if (datos.Precio <= 0)
            return "El precio debe ser mayor que 0.";

        if (datos.Stock < 0)
            return "El stock debe ser mayor o igual a 0.";

        if (string.IsNullOrWhiteSpace(datos.Categoria))
            return "La categoría es obligatoria.";

        return null;
    }

    private static ProductoResponse Mapear(Producto producto) => new()
    {
        Id = producto.Id,
        Codigo = producto.Codigo,
        Nombre = producto.Nombre,
        Precio = producto.Precio,
        Stock = producto.Stock,
        Categoria = producto.Categoria
    };

    private sealed record DatosProducto(
        string Codigo,
        string Nombre,
        decimal Precio,
        int Stock,
        string Categoria);
}
