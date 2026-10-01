namespace MiniCrm.Domain.Exceptions;

public class CuitDuplicadoException : Exception
{
    public CuitDuplicadoException()
        : base("Ya existe un cliente con ese CUIT.")
    {
    }
}
