using API.Shared.Exceptions;

namespace API.Veiculos.Exceptions
{
    public class VehicleStatusConflictException : ConflictException
    {
        public VehicleStatusConflictException(string message) : base(message) { }
    }
}
