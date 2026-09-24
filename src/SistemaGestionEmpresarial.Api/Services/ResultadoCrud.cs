namespace SistemaGestionEmpresarial.Api.Services;

public enum EstadoCrud
{
    Exito,
    Validacion,
    NoEncontrado,
    Conflicto
}

public sealed class ResultadoCrud<T>
{
    private ResultadoCrud(EstadoCrud estado, T? valor, string mensaje)
    {
        Estado = estado;
        Valor = valor;
        Mensaje = mensaje;
    }

    public EstadoCrud Estado { get; }
    public T? Valor { get; }
    public string Mensaje { get; }
    public bool EsExitoso => Estado == EstadoCrud.Exito;

    public static ResultadoCrud<T> Exito(T valor, string mensaje = "Operación realizada correctamente.") =>
        new(EstadoCrud.Exito, valor, mensaje);

    public static ResultadoCrud<T> Validacion(string mensaje) => new(EstadoCrud.Validacion, default, mensaje);
    public static ResultadoCrud<T> NoEncontrado(string mensaje) => new(EstadoCrud.NoEncontrado, default, mensaje);
    public static ResultadoCrud<T> Conflicto(string mensaje) => new(EstadoCrud.Conflicto, default, mensaje);
}
