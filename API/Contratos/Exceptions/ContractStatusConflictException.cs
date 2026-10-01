using API.Shared.Exceptions;

namespace API.Contratos.Exceptions
{
    public class ContractStatusConflictException : ConflictException
    {
        public ContractStatusConflictException(string message) : base(message) { }
    }
}
