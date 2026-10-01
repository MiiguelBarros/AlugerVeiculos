using API.Shared.Exceptions;

namespace API.Utilizadores.Exceptions
{
    public class SelfDeactivationException : BusinessRuleException
    {
        public SelfDeactivationException(string message) : base(message) { }
    }
}
