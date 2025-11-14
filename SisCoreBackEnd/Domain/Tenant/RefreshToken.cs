namespace SisCoreBackEnd.Domain.Tenant
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Jti { get; set; } = default!; // JWT ID único
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByJti { get; set; } // Token que reemplazó este (rotación)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
