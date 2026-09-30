using API.Clientes.DTOs;
using API.Clientes.Exceptions;
using API.Clientes.Interfaces;
using API.Data;
using API.Models;
using API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Clientes.Services
{
    public class ClientService : IClientService
    {
        private readonly AlugerVeiculosContext context;

        public ClientService(AlugerVeiculosContext context)
        {
            this.context = context;
        }

        public async Task<ICollection<ClientDTO>> GetAllAsync(ClientFilterDTO filters)
        {
            IQueryable<Client> query = context.Clients.AsNoTracking();

            if (filters.Status != null)
                query = query.Where(c => c.Status == filters.Status);

            var clients = await query
                .OrderBy(c => c.FullName)
                .ToListAsync();

            return ClientDTO.FromModelList(clients);
        }

        public async Task<ClientDTO> GetByIdAsync(int clientId)
        {
            var client = await context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null)
                throw new ClientNotFoundException("Cliente não encontrado.");

            return ClientDTO.FromModel(client);
        }

        public async Task<ClientDTO> CreateAsync(ClientRequestDTO dto)
        {
            var email = dto.Email.Trim();
            var driverLicenseNumber = dto.DriverLicenseNumber.Trim().ToUpperInvariant();

            await ValidateUniqueFieldsAsync(email, driverLicenseNumber, null);

            var client = new Client
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                Phone = dto.Phone,
                DriverLicenseNumber = driverLicenseNumber,
                Status = RecordStatus.Active
            };

            await context.Clients.AddAsync(client);
            await context.SaveChangesAsync();

            return ClientDTO.FromModel(client);
        }

        public async Task<ClientDTO> UpdateAsync(int clientId, ClientRequestDTO dto)
        {
            var client = await context.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null)
                throw new ClientNotFoundException("Cliente não encontrado.");

            var email = dto.Email.Trim();
            var driverLicenseNumber = dto.DriverLicenseNumber.Trim().ToUpperInvariant();

            await ValidateUniqueFieldsAsync(email, driverLicenseNumber, clientId);

            client.FullName = dto.FullName.Trim();
            client.Email = email;
            client.Phone = dto.Phone;
            client.DriverLicenseNumber = driverLicenseNumber;

            await context.SaveChangesAsync();

            return ClientDTO.FromModel(client);
        }

        public async Task DeactivateAsync(int clientId)
        {
            var client = await context.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null)
                throw new ClientNotFoundException("Cliente não encontrado.");

            if (client.Status == RecordStatus.Inactive)
                throw new ClientStatusConflictException("O cliente já está inativo.");

            var hasPendingContracts = await context.Contracts.AnyAsync(c =>
                c.ClientId == clientId &&
                c.CancelledAt == null &&
                c.ReturnedAt == null);

            if (hasPendingContracts)
                throw new ClientHasPendingContractsException("Não é possível desativar um cliente com contratos agendados, ativos ou em atraso.");

            client.Status = RecordStatus.Inactive;

            await context.SaveChangesAsync();
        }

        public async Task ReactivateAsync(int clientId)
        {
            var client = await context.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null)
                throw new ClientNotFoundException("Cliente não encontrado.");

            if (client.Status == RecordStatus.Active)
                throw new ClientStatusConflictException("O cliente já está ativo.");

            client.Status = RecordStatus.Active;

            await context.SaveChangesAsync();
        }

        private async Task ValidateUniqueFieldsAsync(string email, string driverLicenseNumber, int? clientId)
        {
            if (await context.Clients.AnyAsync(c => c.Email == email && c.ClientId != clientId))
                throw new ClientEmailAlreadyExistsException("Já existe um cliente com este email.");

            if (await context.Clients.AnyAsync(c => c.DriverLicenseNumber == driverLicenseNumber && c.ClientId != clientId))
                throw new DriverLicenseAlreadyExistsException("Já existe um cliente com este número de carta de condução.");
        }
    }
}
