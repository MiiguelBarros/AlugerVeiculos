using API.Shared.Exceptions;

namespace API.Utilizadores.Exceptions
{
    public class UserStatusConflictException : ConflictException
    {
        public UserStatusConflictException(string message) : base(message) { }
    }
}
