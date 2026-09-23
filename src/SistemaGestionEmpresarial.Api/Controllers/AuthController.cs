using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Api.Services;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Campos requeridos incompletos.";

            return BadRequest(LoginResponse.Fail(firstError));
        }

        var response = await _authService.LoginAsync(request);

        return response.Success
            ? Ok(response)
            : Unauthorized(response);
    }
}
