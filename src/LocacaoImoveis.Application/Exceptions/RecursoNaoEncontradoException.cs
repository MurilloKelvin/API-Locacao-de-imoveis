namespace LocacaoImoveis.Application.Exceptions;

public sealed class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string recurso, object identificador)
        : base($"{recurso} com identificador '{identificador}' não foi encontrado.")
    {
    }
}