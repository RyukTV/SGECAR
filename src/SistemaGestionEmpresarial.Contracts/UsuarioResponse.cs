namespace SistemaGestionEmpresarial.Contracts;

public sealed class UsuarioResponse
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int RolId { get; set; }
    public string NombreRol { get; set; } = string.Empty;
}
