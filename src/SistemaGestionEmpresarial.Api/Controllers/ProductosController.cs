using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Api.Services;
using SistemaGestionEmpresarial.Contracts;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Controllers;

[ApiController]
[Route("api/productos")]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status403Forbidden)]
public sealed class ProductosController : ControllerBase
{
    private readonly IProductosService _productosService;

    public ProductosController(IProductosService productosService)
    {
        _productosService = productosService;
    }

    [HttpGet]
    [Permiso(Permisos.Consultar)]
    [ProducesResponseType(typeof(IReadOnlyList<ProductoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductoResponse>>> ObtenerTodos(
        [FromQuery] string? codigoOId,
        [FromQuery] string? nombre,
        [FromQuery] int? categoriaId,
        CancellationToken cancellationToken) =>
        Ok(await _productosService.ObtenerTodosAsync(codigoOId, nombre, categoriaId, cancellationToken));

    [HttpGet("categorias-disponibles")]
    [Permiso(Permisos.Consultar)]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaProductoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaProductoResponse>>> ObtenerCategoriasDisponibles(
        CancellationToken cancellationToken) =>
        Ok(await _productosService.ObtenerCategoriasDisponiblesAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [Permiso(Permisos.Consultar)]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoResponse>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var producto = await _productosService.ObtenerPorIdAsync(id, cancellationToken);
        return producto is null
            ? NotFound(new MensajeResponse { Message = "El producto solicitado no existe." })
            : Ok(producto);
    }

    [HttpPost]
    [Permiso(Permisos.Agregar)]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoResponse>> Crear(
        [FromBody] CrearProductoRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _productosService.CrearAsync(request, cancellationToken);
        if (!resultado.EsExitoso)
            return ConvertirError(resultado);

        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor!.Id }, resultado.Valor);
    }

    [HttpPut("{id:int}")]
    [Permiso(Permisos.Modificar)]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoResponse>> Actualizar(
        int id,
        [FromBody] ActualizarProductoRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _productosService.ActualizarAsync(id, request, cancellationToken);
        return resultado.EsExitoso ? Ok(resultado.Valor) : ConvertirError(resultado);
    }

    [HttpDelete("{id:int}")]
    [Permiso(Permisos.Eliminar)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _productosService.EliminarAsync(id, cancellationToken);
        return resultado.EsExitoso
            ? Ok(new MensajeResponse { Message = resultado.Mensaje })
            : ConvertirError(resultado);
    }

    private ActionResult ConvertirError<T>(ResultadoCrud<T> resultado) => resultado.Estado switch
    {
        EstadoCrud.NoEncontrado => NotFound(new MensajeResponse { Message = resultado.Mensaje }),
        EstadoCrud.Conflicto => Conflict(new MensajeResponse { Message = resultado.Mensaje }),
        _ => BadRequest(new MensajeResponse { Message = resultado.Mensaje })
    };
}
