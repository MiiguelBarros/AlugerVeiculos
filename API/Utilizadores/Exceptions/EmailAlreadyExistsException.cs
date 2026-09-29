using API.Shared.Exceptions;

namespace API.Utilizadores.Exceptions
{
    public class EmailAlreadyExistsException : ConflictException
    {
        public EmailAlreadyExistsException(string message) : base(message) { }
    }
}
