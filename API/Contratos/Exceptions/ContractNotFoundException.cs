using API.Shared.Exceptions;

namespace API.Contratos.Exceptions
{
    public class ContractNotFoundException : NotFoundException
    {
        public ContractNotFoundException(string message) : base(message) { }
    }
}
