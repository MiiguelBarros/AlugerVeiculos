using API.Models.Enums;

namespace API.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        
        public string FullName { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
        
        public string DriverLicenseNumber { get; set; } = string.Empty;

        public RecordStatus Status { get; set; } = RecordStatus.Active;

        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
