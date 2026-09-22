using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger con soporte de JWT: los endpoints protegidos solo pueden probarse enviando el token.
builder.Services.AddSwaggerGen(options =>
{
    var esquemaJwt = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue aquí el token devuelto por api/auth/login.",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, esquemaJwt);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquemaJwt] = Array.Empty<string>() });
});

// Base de Datos (Gestionada por el compañero de equipo)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.")));

// Servicio de Autenticación
builder.Services.AddScoped<IAuthService, AuthService>();

// Operaciones de la Etapa I, protegidas por permiso
builder.Services.AddScoped<IOperacionesService, OperacionesService>();

// Configuración de Autenticación y JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta la clave de seguridad Jwt:Key en la configuración.");
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

// Control de acceso por rol: una política por permiso y denegación por omisión.
builder.Services.AddAutorizacionPorPermisos();

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

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(dbContext);
}

app.UseCors(developmentCorsPolicy);

// Antes de la autorización: convierte en 403 las denegaciones lanzadas desde los Services.
app.UseMiddleware<MiddlewareDePermisoDenegado>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
