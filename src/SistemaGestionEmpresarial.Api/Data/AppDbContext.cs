using Microsoft.EntityFrameworkCore;
using SistemaGestionEmpresarial.Api.Entities;

namespace SistemaGestionEmpresarial.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<CategoriaProducto> CategoriasProducto => Set<CategoriaProducto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(rol => rol.Id);
            entity.Property(rol => rol.Nombre).IsRequired().HasMaxLength(100);
            entity.HasIndex(rol => rol.Nombre).IsUnique();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(usuario => usuario.Id);
            entity.Property(usuario => usuario.NombreUsuario).IsRequired().HasMaxLength(100);
            entity.HasIndex(usuario => usuario.NombreUsuario).IsUnique();
            entity.Property(usuario => usuario.NombreCompleto).IsRequired().HasMaxLength(150);
            entity.Property(usuario => usuario.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(usuario => usuario.IntentosFallidos).HasDefaultValue(0);
            entity.HasOne(usuario => usuario.Rol)
                .WithMany()
                .HasForeignKey(usuario => usuario.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(cliente => cliente.Id);
            entity.Property(cliente => cliente.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(cliente => cliente.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(cliente => cliente.Telefono).IsRequired().HasMaxLength(30);
            entity.Property(cliente => cliente.Correo).IsRequired().HasMaxLength(150);
            entity.HasIndex(cliente => cliente.Correo).IsUnique();
            entity.Property(cliente => cliente.Direccion).IsRequired().HasMaxLength(250);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Productos");
            entity.HasKey(producto => producto.Id);
            entity.Property(producto => producto.Codigo).IsRequired().HasMaxLength(50);
            entity.HasIndex(producto => producto.Codigo).IsUnique();
            entity.Property(producto => producto.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(producto => producto.Precio).HasPrecision(18, 2);
            entity.HasOne(producto => producto.Categoria)
                .WithMany(categoria => categoria.Productos)
                .HasForeignKey(producto => producto.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CategoriaProducto>(entity =>
        {
            entity.ToTable("CategoriasProducto");
            entity.HasKey(categoria => categoria.Id);
            entity.Property(categoria => categoria.Nombre).IsRequired().HasMaxLength(100);
            entity.HasIndex(categoria => categoria.Nombre).IsUnique();
        });
    }
}
