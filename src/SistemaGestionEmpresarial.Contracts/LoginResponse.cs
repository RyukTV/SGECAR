namespace SistemaGestionEmpresarial.Contracts;

public sealed class LoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public string? Username { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Role { get; set; }

    public static LoginResponse Ok(string token, string username, string nombreCompleto, string role, string message = "Inicio de sesión exitoso.")
    {
        return new LoginResponse
        {
            Success = true,
            Token = token,
            Username = username,
            NombreCompleto = nombreCompleto,
            Role = role,
            Message = message
        };
    }

    public static LoginResponse Fail(string message)
    {
        return new LoginResponse
        {
            Success = false,
            Message = message
        };
    }
}
