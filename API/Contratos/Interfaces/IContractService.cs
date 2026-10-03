using API.Contratos.DTOs;

namespace API.Contratos.Interfaces
{
    public interface IContractService
    {
        public Task<ICollection<ContractDTO>> GetAllAsync(ContractFilterDTO filters);

        public Task<ContractDTO> GetByIdAsync(int contractId);

        public Task<ContractDTO> CreateAsync(CreateContractDTO dto, int userId);

        public Task<ContractDTO> ReturnAsync(int contractId, ReturnContractDTO dto);

        public Task<ContractDTO> CancelAsync(int contractId);
    }
}
