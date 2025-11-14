namespace SisCoreBackEnd.Domain.Tenant
{
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; } // NULL si solo usa teléfono
        public string? PhoneNumber { get; set; } // NULL si solo usa email
        public string? PasswordHash { get; set; } // NULL si solo usa OAuth/teléfono
        public string? GoogleId { get; set; } // ID de Google OAuth
        public string FullName { get; set; } = default!;
        public string? EmployeeNumber { get; set; }
        public bool PhoneVerified { get; set; } = false;
        public bool EmailVerified { get; set; } = false;
        public string? MfaSecret { get; set; } // TOTP secret para MFA
        public bool MfaEnabled { get; set; } = false;
        public byte Status { get; set; } = 1; // 1=Activo, 0=Inactivo, 2=Bloqueado
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<Timesheet> Timesheets { get; set; } = new List<Timesheet>();
    }
}
