using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaGestionEmpresarial.Api.Authorization;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOperacionesService, OperacionesService>();
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IRolesService, RolesService>();

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
app.UseMiddleware<MiddlewareDePermisoDenegado>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
