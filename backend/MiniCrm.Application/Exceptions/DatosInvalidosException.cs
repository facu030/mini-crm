namespace MiniCrm.Application.Exceptions;

public class DatosInvalidosException : Exception
{
    public Dictionary<string, string[]> Errores { get; }

    public DatosInvalidosException(Dictionary<string, string[]> errores)
        : base("Revisá los datos ingresados.")
    {
        Errores = errores;
    }
}
