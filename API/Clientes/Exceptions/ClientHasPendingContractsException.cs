using API.Shared.Exceptions;

namespace API.Clientes.Exceptions
{
    public class ClientHasPendingContractsException : ConflictException
    {
        public ClientHasPendingContractsException(string message) : base(message) { }
    }
}
