using API.Shared.Exceptions;

namespace API.AutenticacaoAutorizacao.Exceptions
{
    public class InvalidCredentialsException : UnauthorizedException
    {
        public InvalidCredentialsException(string message) : base(message) { }
    }
}
