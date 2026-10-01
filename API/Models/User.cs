using API.Models.Enums;

namespace API.Models
{
    public class User
    {
        public int UserId { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string PasswordHash { get; set; } = string.Empty;
        
        public Role Role { get; set; }

        public RecordStatus Status { get; set; } = RecordStatus.Active;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
