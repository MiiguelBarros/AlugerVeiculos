using API.Models;
using API.Models.Enums;

namespace API.Clientes.DTOs
{
    public class ClientDTO
    {
        public int ClientId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string DriverLicenseNumber { get; set; } = string.Empty;

        public RecordStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public static ICollection<ClientDTO> FromModelList(ICollection<Client> models)
        {
            ICollection<ClientDTO> dtos = new List<ClientDTO>();
            foreach (var model in models)
            {
                dtos.Add(FromModel(model));
            }

            return dtos;
        }

        public static ClientDTO FromModel(Client model)
        {
            return new ClientDTO
            {
                ClientId = model.ClientId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                DriverLicenseNumber = model.DriverLicenseNumber,
                Status = model.Status,
                CreatedAt = model.CreatedAt
            };
        }
    }
}
