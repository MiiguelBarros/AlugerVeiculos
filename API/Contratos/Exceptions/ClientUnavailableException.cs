using API.Shared.Exceptions;

namespace API.Contratos.Exceptions
{
    public class ClientUnavailableException : ConflictException
    {
        public ClientUnavailableException(string message) : base(message) { }
    }
}
