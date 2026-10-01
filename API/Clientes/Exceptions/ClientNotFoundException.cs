using API.Shared.Exceptions;

namespace API.Clientes.Exceptions
{
    public class ClientNotFoundException : NotFoundException
    {
        public ClientNotFoundException(string message) : base(message) { }
    }
}
