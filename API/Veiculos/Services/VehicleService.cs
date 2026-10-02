using API.Data;
using API.Models;
using API.Models.Enums;
using API.Veiculos.DTOs;
using API.Veiculos.Exceptions;
using API.Veiculos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Veiculos.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly AlugerVeiculosContext context;

        public VehicleService(AlugerVeiculosContext context)
        {
            this.context = context;
        }

        public async Task<ICollection<VehicleDTO>> GetAllAsync(VehicleFilterDTO filters)
        {
            IQueryable<Vehicle> query = context.Vehicles
                .AsNoTracking()
                .Include(v => v.Contracts);

            if (filters.Status != null)
                query = query.Where(v => v.Status == filters.Status);

            var vehicles = await query
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .ThenBy(v => v.LicensePlate)
                .ToListAsync();

            var dtos = VehicleDTO.FromModelList(vehicles, Today());

            if (filters.Availability != null)
                dtos = dtos.Where(v => v.Availability == filters.Availability).ToList();

            return dtos;
        }

        public async Task<VehicleDTO> GetByIdAsync(int vehicleId)
        {
            var vehicle = await context.Vehicles
                .AsNoTracking()
                .Include(v => v.Contracts)
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

            if (vehicle == null)
                throw new VehicleNotFoundException("Veículo não encontrado.");

            return VehicleDTO.FromModel(vehicle, Today());
        }

        public async Task<VehicleDTO> CreateAsync(VehicleRequestDTO dto)
        {
            var licensePlate = NormalizeLicensePlate(dto.LicensePlate);

            if (await context.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate))
                throw new LicensePlateAlreadyExistsException($"Já existe um veículo com a matrícula {licensePlate}.");

            var vehicle = new Vehicle
            {
                Brand = dto.Brand.Trim(),
                Model = dto.Model.Trim(),
                LicensePlate = licensePlate,
                Year = dto.Year!.Value,
                FuelType = dto.FuelType!.Value,
                Status = RecordStatus.Active
            };

            await context.Vehicles.AddAsync(vehicle);
            await context.SaveChangesAsync();

            return VehicleDTO.FromModel(vehicle, Today());
        }

        public async Task<VehicleDTO> UpdateAsync(int vehicleId, VehicleRequestDTO dto)
        {
            var vehicle = await context.Vehicles
                .Include(v => v.Contracts)
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

            if (vehicle == null)
                throw new VehicleNotFoundException("Veículo não encontrado.");

            var licensePlate = NormalizeLicensePlate(dto.LicensePlate);

            if (await context.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate && v.VehicleId != vehicleId))
                throw new LicensePlateAlreadyExistsException($"Já existe um veículo com a matrícula {licensePlate}.");

            vehicle.Brand = dto.Brand.Trim();
            vehicle.Model = dto.Model.Trim();
            vehicle.LicensePlate = licensePlate;
            vehicle.Year = dto.Year!.Value;
            vehicle.FuelType = dto.FuelType!.Value;

            await context.SaveChangesAsync();

            return VehicleDTO.FromModel(vehicle, Today());
        }

        public async Task DeactivateAsync(int vehicleId)
        {
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

            if (vehicle == null)
                throw new VehicleNotFoundException("Veículo não encontrado.");

            if (vehicle.Status == RecordStatus.Inactive)
                throw new VehicleStatusConflictException("O veículo já está inativo.");

            var hasPendingContracts = await context.Contracts.AnyAsync(c =>
                c.VehicleId == vehicleId &&
                c.CancelledAt == null &&
                c.ReturnedAt == null);

            if (hasPendingContracts)
                throw new VehicleHasPendingContractsException("Não é possível desativar um veículo com contratos agendados, ativos ou em atraso.");

            vehicle.Status = RecordStatus.Inactive;

            await context.SaveChangesAsync();
        }

        public async Task ReactivateAsync(int vehicleId)
        {
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

            if (vehicle == null)
                throw new VehicleNotFoundException("Veículo não encontrado.");

            if (vehicle.Status == RecordStatus.Active)
                throw new VehicleStatusConflictException("O veículo já está ativo.");

            vehicle.Status = RecordStatus.Active;

            await context.SaveChangesAsync();
        }

        private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);

        private static string NormalizeLicensePlate(string licensePlate)
        {
            var characters = new string(licensePlate.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

            return $"{characters[..2]}-{characters[2..4]}-{characters[4..]}";
        }
    }
}
