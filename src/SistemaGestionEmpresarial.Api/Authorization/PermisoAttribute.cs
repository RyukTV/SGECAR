using Microsoft.AspNetCore.Authorization;
using SistemaGestionEmpresarial.Contracts.Autorizacion;

namespace SistemaGestionEmpresarial.Api.Authorization;

/// <summary>
/// Exige un permiso sobre un controlador o una acción, por ejemplo <c>[Permiso(Permisos.Eliminar)]</c>.
/// Evita repetir el nombre de la política en cada endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class PermisoAttribute : AuthorizeAttribute
{
    public PermisoAttribute(string permiso)
    {
        Permiso = permiso;
        Policy = PoliticasDePermiso.Nombre(permiso);
    }

    public string Permiso { get; }
}
