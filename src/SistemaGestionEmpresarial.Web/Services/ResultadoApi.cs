namespace SistemaGestionEmpresarial.Web.Services;

public sealed class ResultadoApi<T>
{
    private ResultadoApi(bool exito, T? valor, string mensaje, int? codigoHttp)
    {
        Exito = exito;
        Valor = valor;
        Mensaje = mensaje;
        CodigoHttp = codigoHttp;
    }

    public bool Exito { get; }
    public T? Valor { get; }
    public string Mensaje { get; }
    public int? CodigoHttp { get; }

    public static ResultadoApi<T> Correcto(T valor, string mensaje = "Operación realizada correctamente.", int? codigoHttp = null) =>
        new(true, valor, mensaje, codigoHttp);

    public static ResultadoApi<T> Fallo(string mensaje, int? codigoHttp = null) =>
        new(false, default, mensaje, codigoHttp);
}
