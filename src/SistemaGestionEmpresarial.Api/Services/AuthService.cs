using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Api.Entities;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly AppDbContext _dbContext;

    // Usuarios iniciales de respaldo requeridos por el Caso Práctico 1 (Etapa I).
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

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return LoginResponse.Fail("Debe ingresar tanto el usuario como la contraseña.");
        }

        var usernameNormalized = request.Username.Trim();

        // 1. Autenticación con la Base de Datos real (AppDbContext con entidades Usuario y Rol)
        try
        {
            var dbUser = await _dbContext.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario.ToLower() == usernameNormalized.ToLower());

            if (dbUser is not null)
            {
                // Validación de estado activo
                if (!dbUser.Activo)
                {
                    _logger.LogWarning("Intento de login fallido: usuario '{Username}' está inactivo en BD.", usernameNormalized);
                    return LoginResponse.Fail("El usuario se encuentra inactivo. Contacte al Administrador.");
                }

                // Validación de bloqueo temporal por intentos fallidos
                if (dbUser.BloqueadoHasta.HasValue && dbUser.BloqueadoHasta.Value > DateTime.UtcNow)
                {
                    _logger.LogWarning("Intento de login fallido: usuario '{Username}' bloqueado temporalmente en BD.", usernameNormalized);
                    return LoginResponse.Fail("El usuario se encuentra bloqueado temporalmente por exceso de intentos fallidos.");
                }

                // Comprobación de contraseña hasheada
                var hasher = new PasswordHasher<Usuario>();
                var verificationResult = hasher.VerifyHashedPassword(dbUser, dbUser.PasswordHash, request.Password);

                if (verificationResult == PasswordVerificationResult.Success || verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    dbUser.IntentosFallidos = 0;
                    dbUser.BloqueadoHasta = null;
                    await _dbContext.SaveChangesAsync();

                    var token = GenerarTokenJwt(dbUser.NombreUsuario, dbUser.NombreCompleto, dbUser.Rol.Nombre);
                    _logger.LogInformation("Inicio de sesión exitoso desde BD para '{Username}' con rol '{Role}'.", dbUser.NombreUsuario, dbUser.Rol.Nombre);

                    return LoginResponse.Ok(
                        token: token,
                        username: dbUser.NombreUsuario,
                        nombreCompleto: dbUser.NombreCompleto,
                        role: dbUser.Rol.Nombre,
                        message: $"¡Bienvenido, {dbUser.NombreCompleto}!"
                    );
                }
                else
                {
                    dbUser.IntentosFallidos++;
                    if (dbUser.IntentosFallidos >= 3)
                    {
                        dbUser.BloqueadoHasta = DateTime.UtcNow.AddMinutes(5);
                    }
                    await _dbContext.SaveChangesAsync();

                    _logger.LogWarning("Intento de login fallido: contraseña incorrecta en BD para '{Username}'.", usernameNormalized);
                    return LoginResponse.Fail("Usuario o contraseña incorrectos.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Consulta a AppDbContext no disponible ({Message}). Utilizando usuarios predefinidos en memoria.", ex.Message);
        }

        // 2. Validación con usuarios de respaldo en memoria si la base de datos no está conectada localmente
        var user = PredefinedUsers.FirstOrDefault(u =>
            string.Equals(u.Username, usernameNormalized, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            _logger.LogWarning("Intento de login fallido: usuario '{Username}' no encontrado.", usernameNormalized);
            return LoginResponse.Fail("Usuario o contraseña incorrectos.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Intento de login fallido: usuario '{Username}' está inactivo.", usernameNormalized);
            return LoginResponse.Fail("El usuario se encuentra inactivo. Contacte al Administrador.");
        }

        if (!string.Equals(user.Password, request.Password))
        {
            _logger.LogWarning("Intento de login fallido: contraseña incorrecta para '{Username}'.", usernameNormalized);
            return LoginResponse.Fail("Usuario o contraseña incorrectos.");
        }

        var fallbackToken = GenerarTokenJwt(user.Username, user.NombreCompleto, user.Role);
        _logger.LogInformation("Inicio de sesión exitoso para '{Username}' con rol '{Role}'.", user.Username, user.Role);

        return LoginResponse.Ok(
            token: fallbackToken,
            username: user.Username,
            nombreCompleto: user.NombreCompleto,
            role: user.Role,
            message: $"¡Bienvenido, {user.NombreCompleto}!"
        );
    }

    private string GenerarTokenJwt(string username, string nombreCompleto, string role)
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
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.Name, nombreCompleto),
            new Claim(ClaimTypes.Role, role)
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
