using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Api.Services;
using SistemaGestionEmpresarial.Contracts;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Permiso(Permisos.GestionarUsuarios)]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(AccesoDenegadoResponse), StatusCodes.Status403Forbidden)]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _usuariosService;

    public UsuariosController(IUsuariosService usuariosService)
    {
        _usuariosService = usuariosService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioResponse>>> ObtenerTodos(
        [FromQuery] string? buscar,
        CancellationToken cancellationToken) =>
        Ok(await _usuariosService.ObtenerTodosAsync(buscar, cancellationToken));

    [HttpGet("roles-disponibles")]
    [ProducesResponseType(typeof(IReadOnlyList<RolOpcionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RolOpcionResponse>>> ObtenerRolesDisponibles(
        CancellationToken cancellationToken) =>
        Ok(await _usuariosService.ObtenerRolesDisponiblesAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var usuario = await _usuariosService.ObtenerPorIdAsync(id, cancellationToken);
        return usuario is null
            ? NotFound(new MensajeResponse { Message = "El usuario solicitado no existe." })
            : Ok(usuario);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> Crear(
        [FromBody] CrearUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _usuariosService.CrearAsync(request, cancellationToken);
        if (!resultado.EsExitoso)
            return ConvertirError(resultado);

        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor!.Id }, resultado.Valor);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> Actualizar(
        int id,
        [FromBody] ActualizarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _usuariosService.ActualizarAsync(id, request, cancellationToken);
        return resultado.EsExitoso ? Ok(resultado.Valor) : ConvertirError(resultado);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MensajeResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var resultado = await _usuariosService.EliminarAsync(id, cancellationToken);
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
