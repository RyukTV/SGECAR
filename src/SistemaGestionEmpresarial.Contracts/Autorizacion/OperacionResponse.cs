namespace SistemaGestionEmpresarial.Contracts.Autorizacion;

/// <summary>Resultado de una operación autorizada por permiso.</summary>
public sealed class OperacionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    /// <summary>Permiso que la operación exigía.</summary>
    public string Accion { get; set; } = string.Empty;

    /// <summary>Rol con el que se autorizó la operación.</summary>
    public string? Rol { get; set; }

    public static OperacionResponse Realizada(string accion, string? rol) => new()
    {
        Success = true,
        Accion = accion,
        Rol = rol,
        Message = $"Operación autorizada: el rol {rol} puede {Permisos.Describir(accion)}."
    };
}
