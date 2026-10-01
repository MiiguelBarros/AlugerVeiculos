using API.Models;
using API.Models.Enums;

namespace API.Utilizadores.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public Role Role { get; set; }

        public RecordStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public static ICollection<UserDTO> FromModelList(ICollection<User> models)
        {
            ICollection<UserDTO> dtos = new List<UserDTO>();
            foreach (var model in models)
            {
                dtos.Add(FromModel(model));
            }

            return dtos;
        }

        public static UserDTO FromModel(User model)
        {
            return new UserDTO
            {
                UserId = model.UserId,
                Name = model.Name,
                Email = model.Email,
                Role = model.Role,
                Status = model.Status,
                CreatedAt = model.CreatedAt
            };
        }
    }
}
