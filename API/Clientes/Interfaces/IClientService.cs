using API.Clientes.DTOs;

namespace API.Clientes.Interfaces
{
    public interface IClientService
    {
        public Task<ICollection<ClientDTO>> GetAllAsync(ClientFilterDTO filters);

        public Task<ClientDTO> GetByIdAsync(int clientId);

        public Task<ClientDTO> CreateAsync(ClientRequestDTO dto);

        public Task<ClientDTO> UpdateAsync(int clientId, ClientRequestDTO dto);

        public Task DeactivateAsync(int clientId);

        public Task ReactivateAsync(int clientId);
    }
}
