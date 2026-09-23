using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public ActionResult<HealthResponse> Get() => new HealthResponse { Status = "ok" };
}
