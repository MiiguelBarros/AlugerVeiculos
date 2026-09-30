using API.Shared.Exceptions;

namespace API.Veiculos.Exceptions
{
    public class VehicleHasPendingContractsException : ConflictException
    {
        public VehicleHasPendingContractsException(string message) : base(message) { }
    }
}
