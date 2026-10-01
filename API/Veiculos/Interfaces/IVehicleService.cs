using API.Veiculos.DTOs;

namespace API.Veiculos.Interfaces
{
    public interface IVehicleService
    {
        public Task<ICollection<VehicleDTO>> GetAllAsync(VehicleFilterDTO filters);

        public Task<VehicleDTO> GetByIdAsync(int vehicleId);

        public Task<VehicleDTO> CreateAsync(VehicleRequestDTO dto);

        public Task<VehicleDTO> UpdateAsync(int vehicleId, VehicleRequestDTO dto);

        public Task DeactivateAsync(int vehicleId);

        public Task ReactivateAsync(int vehicleId);
    }
}
