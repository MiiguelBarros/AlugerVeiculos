using API.Models;
using API.Models.Enums;

namespace API.Contratos.DTOs
{
    public class ContractDTO
    {
        public int ContractId { get; set; }

        public int ClientId { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public int VehicleId { get; set; }

        public string VehicleLicensePlate { get; set; } = string.Empty;

        public string VehicleBrand { get; set; } = string.Empty;

        public string VehicleModel { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public int StartMileage { get; set; }

        public int? EndMileage { get; set; }

        public DateOnly? ReturnedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public ContractStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public static ICollection<ContractDTO> FromModelList(ICollection<Contract> models, DateOnly today)
        {
            ICollection<ContractDTO> dtos = new List<ContractDTO>();
            foreach (var model in models)
            {
                dtos.Add(FromModel(model, today));
            }

            return dtos;
        }

        public static ContractDTO FromModel(Contract model, DateOnly today)
        {
            return new ContractDTO
            {
                ContractId = model.ContractId,
                ClientId = model.ClientId,
                ClientName = model.Client.FullName,
                VehicleId = model.VehicleId,
                VehicleLicensePlate = model.Vehicle.LicensePlate,
                VehicleBrand = model.Vehicle.Brand,
                VehicleModel = model.Vehicle.Model,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                StartMileage = model.StartMileage,
                EndMileage = model.EndMileage,
                ReturnedAt = model.ReturnedAt,
                CancelledAt = model.CancelledAt,
                Status = model.GetStatusOn(today),
                CreatedAt = model.CreatedAt
            };
        }
    }
}
