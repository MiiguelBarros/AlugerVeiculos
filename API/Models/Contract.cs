namespace API.Models
{
    public class Contract
    {
        public int ContractId { get; set; }
        
        public int ClientId { get; set; }
        
        public Client Client { get; set; } = null!;
        
        public int VehicleId { get; set; }
        
        public Vehicle Vehicle { get; set; } = null!;
        
        public DateOnly StartDate { get; set; }
        
        public DateOnly EndDate { get; set; }
        
        public int StartMileage { get; set; }

        public int? EndMileage { get; set; }

        public DateOnly? ReturnedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
