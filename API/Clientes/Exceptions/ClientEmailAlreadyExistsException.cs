using API.Shared.Exceptions;

namespace API.Clientes.Exceptions
{
    public class ClientEmailAlreadyExistsException : ConflictException
    {
        public ClientEmailAlreadyExistsException(string message) : base(message) { }
    }
}
