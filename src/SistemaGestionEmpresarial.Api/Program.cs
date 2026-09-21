using Microsoft.EntityFrameworkCore;
using SistemaGestionEmpresarial.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.")));

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
    app.UseCors(developmentCorsPolicy);
}

app.MapControllers();

app.Run();
