using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Api.Services;
using SistemaGestionEmpresarial.Contracts;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Controllers;

[ApiController]
[Route("api/categorias-productos")]
[Permiso(Permisos.GestionarCategorias)]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status403Forbidden)]
public sealed class CategoriasProductoController : ControllerBase
{
    private readonly ICategoriasProductoService _categoriasService;

    public CategoriasProductoController(ICategoriasProductoService categoriasService)
    {
        _categoriasService = categoriasService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaProductoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaProductoResponse>>> ObtenerTodas(
        [FromQuery] string? buscar,
        CancellationToken cancellationToken) =>
        Ok(await _categoriasService.ObtenerTodasAsync(buscar, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoriaProductoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaProductoResponse>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var categoria = await _categoriasService.ObtenerPorIdAsync(id, cancellationToken);
        return categoria is null
            ? NotFound(new MensajeResponse { Message = "La categoría solicitada no existe." })
            : Ok(categoria);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoriaProductoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaProductoResponse>> Crear(
        [FromBody] CrearCategoriaProductoRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _categoriasService.CrearAsync(request, cancellationToken);
        return resultado.EsExitoso
            ? CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor!.Id }, resultado.Valor)
            : ConvertirError(resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaProductoResponse>> Actualizar(
        int id,
        [FromBody] ActualizarCategoriaProductoRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _categoriasService.ActualizarAsync(id, request, cancellationToken);
        return resultado.EsExitoso ? Ok(resultado.Valor) : ConvertirError(resultado);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _categoriasService.EliminarAsync(id, cancellationToken);
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
