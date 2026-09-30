using API.Shared.Exceptions;

namespace API.Contratos.Exceptions
{
    public class InvalidReturnDateException : BusinessRuleException
    {
        public InvalidReturnDateException(string message) : base(message) { }
    }
}
