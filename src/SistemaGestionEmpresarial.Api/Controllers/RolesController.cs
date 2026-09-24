using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Api.Services;
using SistemaGestionEmpresarial.Contracts;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Permiso(Permisos.GestionarRoles)]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status403Forbidden)]
public sealed class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RolResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RolResponse>>> ObtenerTodos(
        [FromQuery] string? buscar,
        CancellationToken cancellationToken) =>
        Ok(await _rolesService.ObtenerTodosAsync(buscar, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RolResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RolResponse>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var rol = await _rolesService.ObtenerPorIdAsync(id, cancellationToken);
        return rol is null
            ? NotFound(new MensajeResponse { Message = "El rol solicitado no existe." })
            : Ok(rol);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RolResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RolResponse>> Crear(
        [FromBody] CrearRolRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _rolesService.CrearAsync(request, cancellationToken);
        if (!resultado.EsExitoso)
            return ConvertirError(resultado);

        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor!.Id }, resultado.Valor);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RolResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RolResponse>> Actualizar(
        int id,
        [FromBody] ActualizarRolRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _rolesService.ActualizarAsync(id, request, cancellationToken);
        return resultado.EsExitoso ? Ok(resultado.Valor) : ConvertirError(resultado);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _rolesService.EliminarAsync(id, cancellationToken);
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
