namespace SistemaGestionEmpresarial.Api.Entities;

public sealed class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
}
