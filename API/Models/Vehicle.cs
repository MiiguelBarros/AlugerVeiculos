using API.Models.Enums;

namespace API.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        
        public string Brand { get; set; } = string.Empty;
        
        public string Model { get; set; } = string.Empty;
        
        public string LicensePlate { get; set; } = string.Empty;
        
        public int Year { get; set; }
        
        public FuelType FuelType { get; set; }

        public RecordStatus Status { get; set; } = RecordStatus.Active;

        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRentedOn(DateOnly date)
        {
            return Contracts.Any(c => c.IsOngoingOn(date));
        }

        public int? GetLastMileage()
        {
            return Contracts.Max(c => c.EndMileage);
        }
    }
}
