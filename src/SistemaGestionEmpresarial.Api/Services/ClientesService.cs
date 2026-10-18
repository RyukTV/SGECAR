using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SistemaGestionEmpresarial.Api.Data;
using SistemaGestionEmpresarial.Api.Entities;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public sealed class ClientesService : IClientesService
{
    private readonly AppDbContext _dbContext;

    public ClientesService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ClienteResponse>> ObtenerTodosAsync(
        int? id,
        string? buscar,
        string? correo,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Clientes.AsNoTracking().AsQueryable();

        if (id.HasValue)
            query = query.Where(cliente => cliente.Id == id.Value);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim();
            query = query.Where(cliente =>
                cliente.Nombre.Contains(termino) ||
                cliente.Apellido.Contains(termino) ||
                (cliente.Nombre + " " + cliente.Apellido).Contains(termino));
        }

        if (!string.IsNullOrWhiteSpace(correo))
        {
            var terminoCorreo = correo.Trim();
            query = query.Where(cliente => cliente.Correo.Contains(terminoCorreo));
        }

        return await query
            .OrderBy(cliente => cliente.Nombre)
            .ThenBy(cliente => cliente.Apellido)
            .Select(cliente => Mapear(cliente))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClienteResponse?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.Id == id)
            .Select(cliente => Mapear(cliente))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ResultadoCrud<ClienteResponse>> CrearAsync(
        CrearClienteRequest request,
        CancellationToken cancellationToken = default)
    {
        var datos = Normalizar(request.Nombre, request.Apellido, request.Telefono, request.Correo, request.Direccion);
        var error = Validar(datos);
        if (error is not null)
            return ResultadoCrud<ClienteResponse>.Validacion(error);

        if (await _dbContext.Clientes.AnyAsync(cliente => cliente.Correo == datos.Correo, cancellationToken))
            return ResultadoCrud<ClienteResponse>.Conflicto("Ya existe un cliente con ese correo.");

        var cliente = new Cliente
        {
            Nombre = datos.Nombre,
            Apellido = datos.Apellido,
            Telefono = datos.Telefono,
            Correo = datos.Correo,
            Direccion = datos.Direccion
        };

        _dbContext.Clientes.Add(cliente);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ResultadoCrud<ClienteResponse>.Conflicto("Ya existe un cliente con ese correo.");
        }

        return ResultadoCrud<ClienteResponse>.Exito(Mapear(cliente), "Cliente creado correctamente.");
    }

    public async Task<ResultadoCrud<ClienteResponse>> ActualizarAsync(
        int id,
        ActualizarClienteRequest request,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _dbContext.Clientes.SingleOrDefaultAsync(
            entidad => entidad.Id == id,
            cancellationToken);
        if (cliente is null)
            return ResultadoCrud<ClienteResponse>.NoEncontrado("El cliente solicitado no existe.");

        var datos = Normalizar(request.Nombre, request.Apellido, request.Telefono, request.Correo, request.Direccion);
        var error = Validar(datos);
        if (error is not null)
            return ResultadoCrud<ClienteResponse>.Validacion(error);

        if (await _dbContext.Clientes.AnyAsync(
                entidad => entidad.Id != id && entidad.Correo == datos.Correo,
                cancellationToken))
            return ResultadoCrud<ClienteResponse>.Conflicto("Ya existe un cliente con ese correo.");

        cliente.Nombre = datos.Nombre;
        cliente.Apellido = datos.Apellido;
        cliente.Telefono = datos.Telefono;
        cliente.Correo = datos.Correo;
        cliente.Direccion = datos.Direccion;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ResultadoCrud<ClienteResponse>.Conflicto("Ya existe un cliente con ese correo.");
        }

        return ResultadoCrud<ClienteResponse>.Exito(Mapear(cliente), "Cliente actualizado correctamente.");
    }

    public async Task<ResultadoCrud<bool>> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _dbContext.Clientes.SingleOrDefaultAsync(
            entidad => entidad.Id == id,
            cancellationToken);
        if (cliente is null)
            return ResultadoCrud<bool>.NoEncontrado("El cliente solicitado no existe.");

        _dbContext.Clientes.Remove(cliente);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ResultadoCrud<bool>.Exito(true, "Cliente eliminado correctamente.");
    }

    private static DatosCliente Normalizar(
        string nombre,
        string apellido,
        string telefono,
        string correo,
        string direccion) =>
        new(nombre.Trim(), apellido.Trim(), telefono.Trim(), correo.Trim(), direccion.Trim());

    private static string? Validar(DatosCliente datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre)) return "El nombre es obligatorio.";
        if (string.IsNullOrWhiteSpace(datos.Apellido)) return "El apellido es obligatorio.";
        if (string.IsNullOrWhiteSpace(datos.Telefono)) return "El teléfono es obligatorio.";
        if (string.IsNullOrWhiteSpace(datos.Correo)) return "El correo es obligatorio.";
        if (!new EmailAddressAttribute().IsValid(datos.Correo)) return "El correo no tiene un formato válido.";
        if (string.IsNullOrWhiteSpace(datos.Direccion)) return "La dirección es obligatoria.";
        return null;
    }

    private static ClienteResponse Mapear(Cliente cliente) => new()
    {
        Id = cliente.Id,
        Nombre = cliente.Nombre,
        Apellido = cliente.Apellido,
        Telefono = cliente.Telefono,
        Correo = cliente.Correo,
        Direccion = cliente.Direccion
    };

    private sealed record DatosCliente(
        string Nombre,
        string Apellido,
        string Telefono,
        string Correo,
        string Direccion);
}
