using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly AppDbContext _dbContext;

    // Usuarios iniciales requeridos por el Caso Práctico 1 (Etapa I).
    // Esta lista actúa como proveedor en memoria mientras el compañero integra las entidades en AppDbContext.
    private static readonly List<PredefinedUser> PredefinedUsers =
    [
        new("admin", "admin123", "Administrador del Sistema", "Administrador", IsActive: true),
        new("supervisor", "supervisor123", "Supervisor de Operaciones", "Supervisor", IsActive: true),
        new("ejecutor", "ejecutor123", "Ejecutor de Procesos", "Ejecutor", IsActive: true)
    ];

    public AuthService(IConfiguration configuration, ILogger<AuthService> logger, AppDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
    }

    public Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Task.FromResult(LoginResponse.Fail("Debe ingresar tanto el usuario como la contraseña."));
        }

        var usernameNormalized = request.Username.Trim();

        // 1. Búsqueda y validación de existencia de usuario
        // NOTA DE INTEGRACIÓN: Cuando el compañero agregue las entidades Usuario y Rol a AppDbContext,
        // se sustituye esta línea por:
        // var user = await _dbContext.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(...);
        var user = PredefinedUsers.FirstOrDefault(u =>
            string.Equals(u.Username, usernameNormalized, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            _logger.LogWarning("Intento de login fallido: usuario '{Username}' no encontrado.", usernameNormalized);
            return Task.FromResult(LoginResponse.Fail("Usuario o contraseña incorrectos."));
        }

        // 2. Validación de estado activo
        if (!user.IsActive)
        {
            _logger.LogWarning("Intento de login fallido: usuario '{Username}' está inactivo.", usernameNormalized);
            return Task.FromResult(LoginResponse.Fail("El usuario se encuentra inactivo. Contacte al Administrador."));
        }

        // 3. Comprobación de contraseña
        if (!string.Equals(user.Password, request.Password))
        {
            _logger.LogWarning("Intento de login fallido: contraseña incorrecta para '{Username}'.", usernameNormalized);
            return Task.FromResult(LoginResponse.Fail("Usuario o contraseña incorrectos."));
        }

        // 4. Generación del Token JWT firmado con claims
        var token = GenerarTokenJwt(user);

        _logger.LogInformation("Inicio de sesión exitoso para '{Username}' con rol '{Role}'.", user.Username, user.Role);

        return Task.FromResult(LoginResponse.Ok(
            token: token,
            username: user.Username,
            nombreCompleto: user.NombreCompleto,
            role: user.Role,
            message: $"¡Bienvenido, {user.NombreCompleto}!"
        ));
    }

    private string GenerarTokenJwt(PredefinedUser user)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta la configuración Jwt:Key en appsettings.json.");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "SistemaGestionEmpresarial.Api";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "SistemaGestionEmpresarial.Web";
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiresInMinutes", 120);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim(ClaimTypes.Name, user.NombreCompleto),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record PredefinedUser(
        string Username,
        string Password,
        string NombreCompleto,
        string Role,
        bool IsActive);
}
