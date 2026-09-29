using API.Data;
using API.Models;
using API.Models.Enums;
using API.Utilizadores.DTOs;
using API.Utilizadores.Exceptions;
using API.Utilizadores.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Utilizadores.Services
{
    public class UserService : IUserService
    {
        private readonly AlugerVeiculosContext context;

        public UserService(AlugerVeiculosContext context)
        {
            this.context = context;
        }

        public async Task<ICollection<UserDTO>> GetAllAsync()
        {
            var users = await context.Users
                .AsNoTracking()
                .OrderBy(u => u.Name)
                .ToListAsync();

            return UserDTO.FromModelList(users);
        }

        public async Task<UserDTO> GetByIdAsync(int userId)
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new UserNotFoundException("Utilizador não encontrado.");

            return UserDTO.FromModel(user);
        }

        public async Task<UserDTO> CreateAsync(CreateUserDTO dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await context.Users.AnyAsync(u => u.Email == email))
                throw new EmailAlreadyExistsException("Já existe um utilizador com este email.");

            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12),
                Role = dto.Role!.Value,
                Status = RecordStatus.Active
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return UserDTO.FromModel(user);
        }

        public async Task DeactivateAsync(int userId, int currentUserId)
        {
            if (userId == currentUserId)
                throw new SelfDeactivationException("Não pode desativar a sua própria conta.");

            var user = await context.Users
                .Include(u => u.RefreshTokens.Where(rt => rt.RevokedAt == null))
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new UserNotFoundException("Utilizador não encontrado.");

            if (user.Status == RecordStatus.Inactive)
                throw new UserStatusConflictException("O utilizador já está inativo.");

            user.Status = RecordStatus.Inactive;

            foreach (var refreshToken in user.RefreshTokens)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        public async Task ReactivateAsync(int userId)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new UserNotFoundException("Utilizador não encontrado.");

            if (user.Status == RecordStatus.Active)
                throw new UserStatusConflictException("O utilizador já está ativo.");

            user.Status = RecordStatus.Active;

            await context.SaveChangesAsync();
        }
    }
}
