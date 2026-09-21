using Microsoft.EntityFrameworkCore;

namespace SistemaGestionEmpresarial.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
