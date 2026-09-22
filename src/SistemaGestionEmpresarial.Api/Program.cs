using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Base de Datos (Gestionada por el compañero de equipo)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.")));

// Servicio de Autenticación
builder.Services.AddScoped<IAuthService, AuthService>();

// Configuración de Autenticación y JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? "SGECAR_SuperSecret_Jwt_Security_Key_2026_ISO615_Development_Only!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SistemaGestionEmpresarial.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SistemaGestionEmpresarial.Web";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

const string developmentCorsPolicy = "BlazorDevelopment";
builder.Services.AddCors(options =>
    options.AddPolicy(developmentCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5180")
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("SQL Server no disponible localmente ({Message}). Se utilizará el modo de desarrollo con datos de respaldo.", ex.Message);
    }
}

app.UseCors(developmentCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
