using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Api.Entities;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public sealed class UsuariosService : IUsuariosService
{
    private readonly AppDbContext _dbContext;
    private readonly IUsuarioActual _usuarioActual;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public UsuariosService(AppDbContext dbContext, IUsuarioActual usuarioActual)
    {
        _dbContext = dbContext;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<UsuarioResponse>> ObtenerTodosAsync(
        string? buscar,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Usuarios.AsNoTracking().Include(usuario => usuario.Rol).AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            query = query.Where(usuario =>
                usuario.NombreUsuario.Contains(termino) ||
                usuario.NombreCompleto.Contains(termino) ||
                usuario.Rol.Nombre.Contains(termino));
        }

        return await query
            .OrderBy(usuario => usuario.NombreUsuario)
            .Select(usuario => new UsuarioResponse
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Activo = usuario.Activo,
                RolId = usuario.RolId,
                NombreRol = usuario.Rol.Nombre
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UsuarioResponse?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbContext.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.Id == id)
            .Select(usuario => new UsuarioResponse
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Activo = usuario.Activo,
                RolId = usuario.RolId,
                NombreRol = usuario.Rol.Nombre
            })
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<RolOpcionResponse>> ObtenerRolesDisponiblesAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(rol => rol.Nombre)
            .Select(rol => new RolOpcionResponse { Id = rol.Id, Nombre = rol.Nombre })
            .ToListAsync(cancellationToken);

    public async Task<ResultadoCrud<UsuarioResponse>> CrearAsync(
        CrearUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        var nombreUsuario = request.NombreUsuario.Trim();
        var nombreCompleto = request.NombreCompleto.Trim();

        if (string.IsNullOrWhiteSpace(nombreUsuario))
            return ResultadoCrud<UsuarioResponse>.Validacion("El nombre de usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            return ResultadoCrud<UsuarioResponse>.Validacion("El nombre completo es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Password))
            return ResultadoCrud<UsuarioResponse>.Validacion("La contraseña es obligatoria.");

        if (await _dbContext.Usuarios.AnyAsync(
                usuario => usuario.NombreUsuario == nombreUsuario,
                cancellationToken))
            return ResultadoCrud<UsuarioResponse>.Conflicto("Ya existe un usuario con ese nombre.");

        var rol = await _dbContext.Roles.SingleOrDefaultAsync(rol => rol.Id == request.RolId, cancellationToken);
        if (rol is null)
            return ResultadoCrud<UsuarioResponse>.Validacion("El rol seleccionado no existe.");

        var usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = nombreCompleto,
            Activo = request.Activo,
            RolId = rol.Id,
            Rol = rol,
            IntentosFallidos = 0,
            BloqueadoHasta = null
        };
        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Password);

        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ResultadoCrud<UsuarioResponse>.Exito(Mapear(usuario), "Usuario creado correctamente.");
    }

    public async Task<ResultadoCrud<UsuarioResponse>> ActualizarAsync(
        int id,
        ActualizarUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == _usuarioActual.Id && !request.Activo)
            return ResultadoCrud<UsuarioResponse>.Conflicto(
                "No puede desactivar el usuario con el que inició sesión.");

        var usuario = await _dbContext.Usuarios
            .Include(entidad => entidad.Rol)
            .SingleOrDefaultAsync(entidad => entidad.Id == id, cancellationToken);
        if (usuario is null)
            return ResultadoCrud<UsuarioResponse>.NoEncontrado("El usuario solicitado no existe.");

        var nombreUsuario = request.NombreUsuario.Trim();
        var nombreCompleto = request.NombreCompleto.Trim();
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            return ResultadoCrud<UsuarioResponse>.Validacion("El nombre de usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            return ResultadoCrud<UsuarioResponse>.Validacion("El nombre completo es obligatorio.");

        if (await _dbContext.Usuarios.AnyAsync(
                entidad => entidad.Id != id && entidad.NombreUsuario == nombreUsuario,
                cancellationToken))
            return ResultadoCrud<UsuarioResponse>.Conflicto("Ya existe un usuario con ese nombre.");

        var rol = await _dbContext.Roles.SingleOrDefaultAsync(entidad => entidad.Id == request.RolId, cancellationToken);
        if (rol is null)
            return ResultadoCrud<UsuarioResponse>.Validacion("El rol seleccionado no existe.");

        usuario.NombreUsuario = nombreUsuario;
        usuario.NombreCompleto = nombreCompleto;
        usuario.Activo = request.Activo;
        usuario.RolId = rol.Id;
        usuario.Rol = rol;

        if (!string.IsNullOrWhiteSpace(request.NuevaPassword))
            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.NuevaPassword);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ResultadoCrud<UsuarioResponse>.Exito(Mapear(usuario), "Usuario actualizado correctamente.");
    }

    public async Task<ResultadoCrud<bool>> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id == _usuarioActual.Id)
            return ResultadoCrud<bool>.Conflicto(
                "No puede eliminar el usuario con el que inició sesión.");

        var usuario = await _dbContext.Usuarios.SingleOrDefaultAsync(
            entidad => entidad.Id == id,
            cancellationToken);
        if (usuario is null)
            return ResultadoCrud<bool>.NoEncontrado("El usuario solicitado no existe.");

        _dbContext.Usuarios.Remove(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ResultadoCrud<bool>.Exito(true, "Usuario eliminado correctamente.");
    }

    private static UsuarioResponse Mapear(Usuario usuario) => new()
    {
        Id = usuario.Id,
        NombreUsuario = usuario.NombreUsuario,
        NombreCompleto = usuario.NombreCompleto,
        Activo = usuario.Activo,
        RolId = usuario.RolId,
        NombreRol = usuario.Rol.Nombre
    };
}
