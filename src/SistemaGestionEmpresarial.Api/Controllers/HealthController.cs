using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Diagnóstico de conectividad. Público a propósito: debe poder comprobarse sin sesión,
    /// por lo que queda exento de la política de denegación por omisión.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public ActionResult<HealthResponse> Get() => new HealthResponse { Status = "ok" };
}
