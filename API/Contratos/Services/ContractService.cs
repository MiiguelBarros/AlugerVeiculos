using API.Clientes.Exceptions;
using API.Contratos.DTOs;
using API.Contratos.Exceptions;
using API.Contratos.Interfaces;
using API.Data;
using API.Models;
using API.Models.Enums;
using API.Utilizadores.Exceptions;
using API.Veiculos.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace API.Contratos.Services
{
    public class ContractService : IContractService
    {
        private const string DateFormat = "dd/MM/yyyy";
        private readonly AlugerVeiculosContext context;

        public ContractService(AlugerVeiculosContext context)
        {
            this.context = context;
        }

        public async Task<ICollection<ContractDTO>> GetAllAsync(ContractFilterDTO filters)
        {
            IQueryable<Contract> query = context.Contracts
                .AsNoTracking()
                .Include(c => c.Client)
                .Include(c => c.Vehicle)
                .Include(c => c.CreatedByUser);

            if (filters.VehicleId != null)
                query = query.Where(c => c.VehicleId == filters.VehicleId);

            if (filters.ClientId != null)
                query = query.Where(c => c.ClientId == filters.ClientId);

            var contracts = await query
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            var dtos = ContractDTO.FromModelList(contracts, Today());

            if (filters.Status != null)
                dtos = dtos.Where(c => c.Status == filters.Status).ToList();

            return dtos;
        }

        public async Task<ContractDTO> GetByIdAsync(int contractId)
        {
            var contract = await context.Contracts
                .AsNoTracking()
                .Include(c => c.Client)
                .Include(c => c.Vehicle)
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null)
                throw new ContractNotFoundException("Contrato não encontrado.");

            return ContractDTO.FromModel(contract, Today());
        }

        public async Task<ContractDTO> CreateAsync(CreateContractDTO dto, int userId)
        {
            var today = Today();
            var startDate = dto.StartDate!.Value;
            var endDate = dto.EndDate!.Value;

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var createdByUser = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (createdByUser == null)
                throw new UserNotFoundException("Utilizador não encontrado.");

            var client = await context.Clients.FirstOrDefaultAsync(c => c.ClientId == dto.ClientId);

            if (client == null)
                throw new ClientNotFoundException("Cliente não encontrado.");

            if (client.Status != RecordStatus.Active)
                throw new ClientUnavailableException("O cliente está inativo e não pode fazer novos contratos.");

            var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId);

            if (vehicle == null)
                throw new VehicleNotFoundException("Veículo não encontrado.");

            if (vehicle.Status != RecordStatus.Active)
                throw new VehicleUnavailableException("O veículo está inativo e não pode ser alugado.");

            var clientHasOverdueContract = await context.Contracts.AnyAsync(c =>
                c.ClientId == client.ClientId &&
                c.CancelledAt == null &&
                c.ReturnedAt == null &&
                c.EndDate < today);

            if (clientHasOverdueContract)
                throw new ClientUnavailableException("O cliente tem um contrato em atraso e só pode fazer novos contratos depois de devolver o veículo.");

            var pendingContract = await context.Contracts.FirstOrDefaultAsync(c =>
                c.VehicleId == vehicle.VehicleId &&
                c.CancelledAt == null &&
                c.ReturnedAt == null);

            if (pendingContract != null)
                throw new VehicleUnavailableException(
                    $"O veículo tem um contrato por concluir ({pendingContract.StartDate.ToString(DateFormat)} a {pendingContract.EndDate.ToString(DateFormat)}) " +
                    "e só pode ser alugado depois de ser devolvido.");

            var lastReturnDate = await context.Contracts
                .Where(c => c.VehicleId == vehicle.VehicleId && c.ReturnedAt != null)
                .MaxAsync(c => c.ReturnedAt);

            if (lastReturnDate != null && startDate <= lastReturnDate)
                throw new VehicleUnavailableException(
                    $"O veículo foi devolvido a {lastReturnDate.Value.ToString(DateFormat)} e só pode ser alugado a partir do dia seguinte, para preparação.");

            var lastEndMileage = await context.Contracts
                .Where(c => c.VehicleId == vehicle.VehicleId && c.EndMileage != null)
                .MaxAsync(c => c.EndMileage);

            if (lastEndMileage != null && dto.StartMileage < lastEndMileage)
                throw new InvalidMileageException($"A quilometragem inicial não pode ser inferior à última registada para este veículo ({lastEndMileage} km).");

            var contract = new Contract
            {
                Client = client,
                Vehicle = vehicle,
                CreatedByUser = createdByUser,
                StartDate = startDate,
                EndDate = endDate,
                StartMileage = dto.StartMileage!.Value,
                DailyRate = dto.DailyRate!.Value
            };

            await context.Contracts.AddAsync(contract);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ContractDTO.FromModel(contract, today);
        }

        public async Task<ContractDTO> ReturnAsync(int contractId, ReturnContractDTO dto)
        {
            var contract = await context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Vehicle)
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null)
                throw new ContractNotFoundException("Contrato não encontrado.");

            if (contract.CancelledAt != null)
                throw new ContractStatusConflictException("Não é possível devolver um contrato cancelado.");

            if (contract.ReturnedAt != null)
                throw new ContractStatusConflictException("Este contrato já foi devolvido.");

            if (dto.ReturnedAt < contract.StartDate)
                throw new InvalidReturnDateException($"A data de devolução não pode ser anterior à data de início do contrato ({contract.StartDate.ToString(DateFormat)}).");

            if (dto.EndMileage < contract.StartMileage)
                throw new InvalidMileageException($"A quilometragem final não pode ser inferior à inicial ({contract.StartMileage} km).");

            contract.ReturnedAt = dto.ReturnedAt;
            contract.EndMileage = dto.EndMileage;

            await context.SaveChangesAsync();

            return ContractDTO.FromModel(contract, Today());
        }

        public async Task<ContractDTO> CancelAsync(int contractId)
        {
            var contract = await context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Vehicle)
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null)
                throw new ContractNotFoundException("Contrato não encontrado.");

            if (contract.ReturnedAt != null)
                throw new ContractStatusConflictException("Não é possível cancelar um contrato já devolvido.");

            if (contract.CancelledAt != null)
                throw new ContractStatusConflictException("Este contrato já foi cancelado.");

            if (contract.StartDate < Today())
                throw new ContractStatusConflictException("Só é possível cancelar um contrato até ao dia de início.");

            contract.CancelledAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return ContractDTO.FromModel(contract, Today());
        }

        private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);
    }
}
