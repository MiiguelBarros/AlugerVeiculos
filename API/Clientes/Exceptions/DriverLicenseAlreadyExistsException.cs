using API.Shared.Exceptions;

namespace API.Clientes.Exceptions
{
    public class DriverLicenseAlreadyExistsException : ConflictException
    {
        public DriverLicenseAlreadyExistsException(string message) : base(message) { }
    }
}
