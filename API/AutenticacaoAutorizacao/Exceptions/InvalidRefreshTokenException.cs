using API.Shared.Exceptions;

namespace API.AutenticacaoAutorizacao.Exceptions
{
    public class InvalidRefreshTokenException : UnauthorizedException
    {
        public InvalidRefreshTokenException(string message) : base(message) { }
    }
}
