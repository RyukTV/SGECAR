namespace SistemaGestionEmpresarial.Web.Services;

/// <summary>
/// Resultado de invocar una operación protegida de la API, ya interpretado para la interfaz:
/// distingue el éxito del rechazo por permisos y del fallo de comunicación.
/// </summary>
public sealed class ResultadoDeOperacion
{
    private ResultadoDeOperacion(bool autorizada, bool huboError, string mensaje, int? codigoHttp)
    {
        Autorizada = autorizada;
        HuboError = huboError;
        Mensaje = mensaje;
        CodigoHttp = codigoHttp;
    }

    /// <summary>La API aceptó y ejecutó la operación.</summary>
    public bool Autorizada { get; }

    /// <summary>La operación no pudo completarse: falta de permisos, sesión caducada o fallo de red.</summary>
    public bool HuboError { get; }

    public string Mensaje { get; }

    public int? CodigoHttp { get; }

    public static ResultadoDeOperacion Permitida(string mensaje, int codigoHttp) =>
        new(autorizada: true, huboError: false, mensaje, codigoHttp);

    public static ResultadoDeOperacion Rechazada(string mensaje, int codigoHttp) =>
        new(autorizada: false, huboError: true, mensaje, codigoHttp);

    public static ResultadoDeOperacion Fallo(string mensaje, int? codigoHttp = null) =>
        new(autorizada: false, huboError: true, mensaje, codigoHttp);
}
