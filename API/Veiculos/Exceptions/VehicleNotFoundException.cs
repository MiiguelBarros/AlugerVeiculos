using API.Shared.Exceptions;

namespace API.Veiculos.Exceptions
{
    public class VehicleNotFoundException : NotFoundException
    {
        public VehicleNotFoundException(string message) : base(message) { }
    }
}
