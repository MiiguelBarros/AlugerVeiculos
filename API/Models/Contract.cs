using API.Models.Enums;

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

        public bool IsOngoingOn(DateOnly date)
        {
            return CancelledAt == null && ReturnedAt == null && StartDate <= date;
        }

        public ContractStatus GetStatusOn(DateOnly date)
        {
            if (CancelledAt != null)
                return ContractStatus.Cancelled;

            if (ReturnedAt != null)
                return ContractStatus.Completed;

            if (StartDate > date)
                return ContractStatus.Scheduled;

            if (EndDate < date)
                return ContractStatus.Overdue;

            return ContractStatus.Active;
        }
    }
}
