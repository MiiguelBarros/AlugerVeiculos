using API.Shared.Exceptions;

namespace API.Contratos.Exceptions
{
    public class VehicleUnavailableException : ConflictException
    {
        public VehicleUnavailableException(string message) : base(message) { }
    }
}
