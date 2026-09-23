using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Api.Services;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Controllers;

/// <summary>
/// Endpoints de las cuatro operaciones de la práctica, cada uno protegido por su permiso.
/// Sirven para verificar que el backend rechaza por sí mismo lo que el rol no permite, incluso
/// si el cliente fuerza la llamada. Cuando existan las entidades reales, estos endpoints se
/// sustituyen por los controladores de negocio conservando los mismos atributos.
/// </summary>
[ApiController]
[Route("api/operaciones")]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status403Forbidden)]
public sealed class OperacionesController : ControllerBase
{
    private readonly IOperacionesService _operacionesService;

    public OperacionesController(IOperacionesService operacionesService)
    {
        _operacionesService = operacionesService;
    }

    /// <summary>Consultar: permitido a los tres roles.</summary>
    [HttpGet("consultar")]
    [Permiso(Permisos.Consultar)]
    [ProducesResponseType(typeof(OperacionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OperacionResponse>> Consultar(CancellationToken cancellationToken) =>
        await _operacionesService.EjecutarAsync(Permisos.Consultar, cancellationToken);

    /// <summary>Agregar: permitido a Administrador y Ejecutor.</summary>
    [HttpPost("agregar")]
    [Permiso(Permisos.Agregar)]
    [ProducesResponseType(typeof(OperacionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OperacionResponse>> Agregar(CancellationToken cancellationToken) =>
        await _operacionesService.EjecutarAsync(Permisos.Agregar, cancellationToken);

    /// <summary>Modificar: permitido a Administrador y Supervisor.</summary>
    [HttpPut("modificar")]
    [Permiso(Permisos.Modificar)]
    [ProducesResponseType(typeof(OperacionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OperacionResponse>> Modificar(CancellationToken cancellationToken) =>
        await _operacionesService.EjecutarAsync(Permisos.Modificar, cancellationToken);

    /// <summary>Eliminar: permitido únicamente al Administrador.</summary>
    [HttpDelete("eliminar")]
    [Permiso(Permisos.Eliminar)]
    [ProducesResponseType(typeof(OperacionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OperacionResponse>> Eliminar(CancellationToken cancellationToken) =>
        await _operacionesService.EjecutarAsync(Permisos.Eliminar, cancellationToken);
}
