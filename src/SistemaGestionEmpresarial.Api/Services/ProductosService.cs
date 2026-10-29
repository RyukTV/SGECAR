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
        string? codigoOId,
        string? nombre,
        int? categoriaId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Productos
            .AsNoTracking()
            .Include(producto => producto.Categoria)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(codigoOId))
        {
            var termino = codigoOId.Trim();
            if (int.TryParse(termino, out var id))
                query = query.Where(producto => producto.Id == id || producto.Codigo.Contains(termino));
            else
                query = query.Where(producto => producto.Codigo.Contains(termino));
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var termino = nombre.Trim();
            query = query.Where(producto => producto.Nombre.Contains(termino));
        }

        if (categoriaId.HasValue)
            query = query.Where(producto => producto.CategoriaId == categoriaId.Value);

        return await query
            .OrderBy(producto => producto.Codigo)
            .Select(producto => Mapear(producto))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CategoriaProductoResponse>> ObtenerCategoriasDisponiblesAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.CategoriasProducto
            .AsNoTracking()
            .OrderBy(categoria => categoria.Nombre)
            .Select(categoria => new CategoriaProductoResponse
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre
            })
            .ToListAsync(cancellationToken);

    public async Task<ProductoResponse?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbContext.Productos
            .AsNoTracking()
            .Include(producto => producto.Categoria)
            .Where(producto => producto.Id == id)
            .Select(producto => Mapear(producto))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ResultadoCrud<ProductoResponse>> CrearAsync(
        CrearProductoRequest request,
        CancellationToken cancellationToken = default)
    {
        var datos = Normalizar(request.Codigo, request.Nombre, request.Precio, request.Stock, request.CategoriaId);
        var error = Validar(datos);
        if (error is not null)
            return ResultadoCrud<ProductoResponse>.Validacion(error);

        var categoria = await _dbContext.CategoriasProducto.SingleOrDefaultAsync(
            entidad => entidad.Id == datos.CategoriaId, cancellationToken);
        if (categoria is null)
            return ResultadoCrud<ProductoResponse>.Validacion("La categoría seleccionada no existe.");

        if (await _dbContext.Productos.AnyAsync(producto => producto.Codigo == datos.Codigo, cancellationToken))
            return ResultadoCrud<ProductoResponse>.Conflicto("Ya existe un producto con ese código.");

        var producto = new Producto
        {
            Codigo = datos.Codigo,
            Nombre = datos.Nombre,
            Precio = datos.Precio,
            Stock = datos.Stock,
            CategoriaId = datos.CategoriaId,
            Categoria = categoria
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
            entidad => entidad.Id == id,
            cancellationToken);
        if (producto is null)
            return ResultadoCrud<ProductoResponse>.NoEncontrado("El producto solicitado no existe.");

        var datos = Normalizar(request.Codigo, request.Nombre, request.Precio, request.Stock, request.CategoriaId);
        var error = Validar(datos);
        if (error is not null)
            return ResultadoCrud<ProductoResponse>.Validacion(error);

        var categoria = await _dbContext.CategoriasProducto.SingleOrDefaultAsync(
            entidad => entidad.Id == datos.CategoriaId, cancellationToken);
        if (categoria is null)
            return ResultadoCrud<ProductoResponse>.Validacion("La categoría seleccionada no existe.");

        if (await _dbContext.Productos.AnyAsync(
                entidad => entidad.Id != id && entidad.Codigo == datos.Codigo,
                cancellationToken))
            return ResultadoCrud<ProductoResponse>.Conflicto("Ya existe un producto con ese código.");

        producto.Codigo = datos.Codigo;
        producto.Nombre = datos.Nombre;
        producto.Precio = datos.Precio;
        producto.Stock = datos.Stock;
        producto.CategoriaId = datos.CategoriaId;
        producto.Categoria = categoria;

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

    public async Task<ResultadoCrud<bool>> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await _dbContext.Productos.SingleOrDefaultAsync(
            entidad => entidad.Id == id,
            cancellationToken);
        if (producto is null)
            return ResultadoCrud<bool>.NoEncontrado("El producto solicitado no existe.");

        _dbContext.Productos.Remove(producto);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ResultadoCrud<bool>.Exito(true, "Producto eliminado correctamente.");
    }

    private static DatosProducto Normalizar(
        string codigo,
        string nombre,
        decimal precio,
        int stock,
        int categoriaId) =>
        new(codigo.Trim().ToUpperInvariant(), nombre.Trim(), precio, stock, categoriaId);

    private static string? Validar(DatosProducto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Codigo)) return "El código es obligatorio.";
        if (string.IsNullOrWhiteSpace(datos.Nombre)) return "El nombre es obligatorio.";
        if (datos.Precio <= 0) return "El precio debe ser mayor que 0.";
        if (datos.Stock < 0) return "El stock debe ser mayor o igual a 0.";
        if (datos.CategoriaId <= 0) return "Debe seleccionar una categoría.";
        return null;
    }

    private static ProductoResponse Mapear(Producto producto) => new()
    {
        Id = producto.Id,
        Codigo = producto.Codigo,
        Nombre = producto.Nombre,
        Precio = producto.Precio,
        Stock = producto.Stock,
        CategoriaId = producto.CategoriaId,
        Categoria = producto.Categoria.Nombre
    };

    private sealed record DatosProducto(
        string Codigo,
        string Nombre,
        decimal Precio,
        int Stock,
        int CategoriaId);
}
