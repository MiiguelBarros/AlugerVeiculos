using API.Shared.Exceptions;

namespace API.Contratos.Exceptions
{
    public class InvalidMileageException : BusinessRuleException
    {
        public InvalidMileageException(string message) : base(message) { }
    }
}
