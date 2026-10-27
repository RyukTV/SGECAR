using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaGestionEmpresarial.Api.Entities;

namespace SistemaGestionEmpresarial.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        var rolesIniciales = new[]
        {
            new Rol
            {
                Nombre = "Administrador",
                PuedeAgregar = true,
                PuedeModificar = true,
                PuedeEliminar = true,
                PuedeConsultar = true,
                PuedeGestionarUsuarios = true,
                PuedeGestionarRoles = true,
                PuedeGestionarClientes = true
            },
            new Rol
            {
                Nombre = "Supervisor",
                PuedeAgregar = false,
                PuedeModificar = true,
                PuedeEliminar = false,
                PuedeConsultar = true,
                PuedeGestionarUsuarios = false,
                PuedeGestionarRoles = false,
                PuedeGestionarClientes = false
            },
            new Rol
            {
                Nombre = "Ejecutor",
                PuedeAgregar = true,
                PuedeModificar = false,
                PuedeEliminar = false,
                PuedeConsultar = true,
                PuedeGestionarUsuarios = false,
                PuedeGestionarRoles = false,
                PuedeGestionarClientes = false
            }
        };

        foreach (var rol in rolesIniciales)
        {
            if (!await dbContext.Roles.AnyAsync(existente => existente.Nombre == rol.Nombre))
            {
                dbContext.Roles.Add(rol);
            }
        }

        await dbContext.SaveChangesAsync();

        var usuariosIniciales = new[]
        {
            (NombreUsuario: "admin", NombreCompleto: "Administrador del Sistema", Clave: "admin123", NombreRol: "Administrador"),
            (NombreUsuario: "supervisor", NombreCompleto: "Supervisor de Operaciones", Clave: "supervisor123", NombreRol: "Supervisor"),
            (NombreUsuario: "ejecutor", NombreCompleto: "Ejecutor de Procesos", Clave: "ejecutor123", NombreRol: "Ejecutor")
        };

        var passwordHasher = new PasswordHasher<Usuario>();

        foreach (var inicial in usuariosIniciales)
        {
            if (await dbContext.Usuarios.AnyAsync(existente => existente.NombreUsuario == inicial.NombreUsuario))
            {
                continue;
            }

            var rol = await dbContext.Roles.SingleAsync(existente => existente.Nombre == inicial.NombreRol);
            var usuario = new Usuario
            {
                NombreUsuario = inicial.NombreUsuario,
                NombreCompleto = inicial.NombreCompleto,
                Activo = true,
                RolId = rol.Id,
                Rol = rol,
                IntentosFallidos = 0,
                BloqueadoHasta = null
            };

            usuario.PasswordHash = passwordHasher.HashPassword(usuario, inicial.Clave);
            dbContext.Usuarios.Add(usuario);
        }

        await dbContext.SaveChangesAsync();

        if (!await dbContext.Productos.AnyAsync())
        {
            dbContext.Productos.AddRange(
                new Producto
                {
                    Codigo = "PROD-001",
                    Nombre = "Laptop Dell Inspiron 15",
                    Precio = 799.99m,
                    Stock = 15,
                    Categoria = "Tecnología"
                },
                new Producto
                {
                    Codigo = "PROD-002",
                    Nombre = "Monitor Samsung 27 Pulgadas",
                    Precio = 229.50m,
                    Stock = 8,
                    Categoria = "Tecnología"
                },
                new Producto
                {
                    Codigo = "PROD-003",
                    Nombre = "Teclado Mecánico RGB",
                    Precio = 55.00m,
                    Stock = 25,
                    Categoria = "Accesorios"
                },
                new Producto
                {
                    Codigo = "PROD-004",
                    Nombre = "Silla Ergonómica de Oficina",
                    Precio = 185.00m,
                    Stock = 12,
                    Categoria = "Mobiliario"
                },
                new Producto
                {
                    Codigo = "PROD-005",
                    Nombre = "Mouse Inalámbrico Logitech",
                    Precio = 29.99m,
                    Stock = 40,
                    Categoria = "Accesorios"
                }
            );

            await dbContext.SaveChangesAsync();
        }
    }
}
