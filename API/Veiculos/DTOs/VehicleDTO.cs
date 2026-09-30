using API.Models;
using API.Models.Enums;

namespace API.Veiculos.DTOs
{
    public class VehicleDTO
    {
        public int VehicleId { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string LicensePlate { get; set; } = string.Empty;

        public int Year { get; set; }

        public FuelType FuelType { get; set; }

        public RecordStatus Status { get; set; }

        public VehicleAvailability Availability { get; set; }

        public DateTime CreatedAt { get; set; }

        public static ICollection<VehicleDTO> FromModelList(ICollection<Vehicle> models, DateOnly today)
        {
            ICollection<VehicleDTO> dtos = new List<VehicleDTO>();
            foreach (var model in models)
            {
                dtos.Add(FromModel(model, today));
            }

            return dtos;
        }

        public static VehicleDTO FromModel(Vehicle model, DateOnly today)
        {
            return new VehicleDTO
            {
                VehicleId = model.VehicleId,
                Brand = model.Brand,
                Model = model.Model,
                LicensePlate = model.LicensePlate,
                Year = model.Year,
                FuelType = model.FuelType,
                Status = model.Status,
                Availability = model.IsRentedOn(today) ? VehicleAvailability.Rented : VehicleAvailability.Available,
                CreatedAt = model.CreatedAt
            };
        }
    }
}
