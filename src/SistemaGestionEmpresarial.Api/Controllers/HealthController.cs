using Microsoft.AspNetCore.Mvc;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<HealthResponse> Get() => new HealthResponse { Status = "ok" };
}
