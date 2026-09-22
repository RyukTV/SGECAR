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
    private const int MaxIntentosPermitidos = 3;
    private const int MinutosBloqueo = 1;

    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly AppDbContext _dbContext;

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

        // 1. Consultar usuario real desde AppDbContext incluyendo su Rol
        var user = await _dbContext.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario.ToLower() == usernameNormalized.ToLower());

        if (user is null)
        {
            _logger.LogWarning("Intento de login fallido: usuario '{Username}' no encontrado.", usernameNormalized);
            return LoginResponse.Fail("Usuario o contraseña incorrectos.");
        }

        // 2. Validar estado activo
        if (!user.Activo)
        {
            _logger.LogWarning("Intento de login fallido: usuario '{Username}' está inactivo.", user.NombreUsuario);
            return LoginResponse.Fail("El usuario se encuentra inactivo. Contacte al Administrador.");
        }

        // 3. Validar si el usuario está bloqueado por intentos fallidos
        if (user.BloqueadoHasta.HasValue)
        {
            if (user.BloqueadoHasta.Value > DateTime.UtcNow)
            {
                var minutosRestantes = Math.Ceiling((user.BloqueadoHasta.Value - DateTime.UtcNow).TotalMinutes);
                _logger.LogWarning("Intento de login fallido: usuario '{Username}' bloqueado temporalmente hasta {BloqueadoHasta}.", user.NombreUsuario, user.BloqueadoHasta.Value);
                return LoginResponse.Fail($"El usuario se encuentra bloqueado temporalmente por seguridad. Intente nuevamente en {minutosRestantes} {(minutosRestantes == 1 ? "minuto" : "minutos")}.");
            }
            else
            {
                // El tiempo de bloqueo ya expiró: limpiar el bloqueo
                user.BloqueadoHasta = null;
                user.IntentosFallidos = 0;
                await _dbContext.SaveChangesAsync();
            }
        }

        // 4. Verificar contraseña con PasswordHasher<Usuario>
        var passwordHasher = new PasswordHasher<Usuario>();
        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            user.IntentosFallidos++;

            if (user.IntentosFallidos >= MaxIntentosPermitidos)
            {
                user.BloqueadoHasta = DateTime.UtcNow.AddMinutes(MinutosBloqueo);
                _logger.LogWarning("Usuario '{Username}' bloqueado hasta {BloqueadoHasta} por alcanzar {Intentos} intentos fallidos.", user.NombreUsuario, user.BloqueadoHasta.Value, user.IntentosFallidos);
            }

            await _dbContext.SaveChangesAsync();

            if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value > DateTime.UtcNow)
            {
                return LoginResponse.Fail($"Ha superado el límite de {MaxIntentosPermitidos} intentos fallidos permitidos. Su acceso ha sido bloqueado temporalmente por {MinutosBloqueo} minuto.");
            }

            var restantes = MaxIntentosPermitidos - user.IntentosFallidos;
            return LoginResponse.Fail($"Usuario o contraseña incorrectos. Le restan {restantes} {(restantes == 1 ? "intento" : "intentos")} antes del bloqueo.");
        }

        // 5. Si la verificación requiere rehash (por actualización de algoritmo), actualizar hash
        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        }

        // 6. Login exitoso: reiniciar IntentosFallidos y BloqueadoHasta
        user.IntentosFallidos = 0;
        user.BloqueadoHasta = null;
        await _dbContext.SaveChangesAsync();

        // 7. Generar Token JWT utilizando user.Id, nombre y rol
        var token = GenerarTokenJwt(user);

        _logger.LogInformation("Inicio de sesión exitoso para usuario '{Username}' (Id: {Id}) con rol '{Role}'.", user.NombreUsuario, user.Id, user.Rol.Nombre);

        return LoginResponse.Ok(
            token: token,
            username: user.NombreUsuario,
            nombreCompleto: user.NombreCompleto,
            role: user.Rol.Nombre,
            message: $"¡Bienvenido, {user.NombreCompleto}!"
        );
    }

    private string GenerarTokenJwt(Usuario user)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta la configuración Jwt:Key.");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "SistemaGestionEmpresarial.Api";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "SistemaGestionEmpresarial.Web";
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiresInMinutes", 120);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.NombreUsuario),
            new Claim(ClaimTypes.GivenName, user.NombreCompleto),
            new Claim(ClaimTypes.Role, user.Rol.Nombre),
            new Claim("rol", user.Rol.Nombre),
            new Claim("usuario", user.NombreUsuario)
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
}
