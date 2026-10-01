using API.Shared.Exceptions;

namespace API.Clientes.Exceptions
{
    public class ClientStatusConflictException : ConflictException
    {
        public ClientStatusConflictException(string message) : base(message) { }
    }
}
