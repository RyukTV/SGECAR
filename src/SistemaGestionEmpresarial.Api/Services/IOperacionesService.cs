using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Services;

/// <summary>
/// Operaciones genéricas de la Etapa I (consultar, agregar, modificar, eliminar) usadas para
/// comprobar el control de acceso de extremo a extremo mientras se define el modelo de datos.
/// Cuando existan las entidades reales, cada método de negocio conservará la misma forma:
/// exigir el permiso y después ejecutar la regla.
/// </summary>
public interface IOperacionesService
{
    Task<OperacionResponse> EjecutarAsync(string permiso, CancellationToken cancellationToken = default);
}
