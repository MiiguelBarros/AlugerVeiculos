using API.Shared.Exceptions;

namespace API.Veiculos.Exceptions
{
    public class LicensePlateAlreadyExistsException : ConflictException
    {
        public LicensePlateAlreadyExistsException(string message) : base(message) { }
    }
}
