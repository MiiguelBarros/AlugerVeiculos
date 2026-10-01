using API.Shared.Exceptions;

namespace API.Utilizadores.Exceptions
{
    public class UserNotFoundException : NotFoundException
    {
        public UserNotFoundException(string message) : base(message) { }
    }
}
