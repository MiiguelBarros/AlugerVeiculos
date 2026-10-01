using API.Shared.Exceptions;

namespace API.AutenticacaoAutorizacao.Exceptions
{
    public class InactiveAccountException : ForbiddenException
    {
        public InactiveAccountException(string message) : base(message) { }
    }
}
