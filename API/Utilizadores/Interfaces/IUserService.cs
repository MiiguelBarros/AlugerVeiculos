using API.Utilizadores.DTOs;

namespace API.Utilizadores.Interfaces
{
    public interface IUserService
    {
        public Task<ICollection<UserDTO>> GetAllAsync();

        public Task<UserDTO> GetByIdAsync(int userId);

        public Task<UserDTO> CreateAsync(CreateUserDTO dto);

        public Task DeactivateAsync(int userId, int currentUserId);

        public Task ReactivateAsync(int userId);
    }
}
