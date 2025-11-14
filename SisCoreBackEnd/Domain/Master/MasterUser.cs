namespace SisCoreBackEnd.Domain.Master
{
    public class MasterUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        
        // Referencias al usuario en la BD tenant (no duplicamos datos)
        public int TenantUserId { get; set; } // ID del usuario en la BD tenant (ej: Users.Id en SisCore)
        public int TenantCompanyId { get; set; } // ID de la empresa (Companies.Id)
        
        // Datos básicos (opcionales, se pueden obtener del tenant)
        public string? PhoneNumber { get; set; }
        public string? GoogleId { get; set; } // ID de Google OAuth (si aplica)
        
        // Estado y verificación
        public bool IsGod { get; set; } = false; // Rol God: tiene todos los privilegios
        public byte Status { get; set; } = 1; // 1=Activo, 0=Inactivo, 2=Bloqueado
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; } // ID del usuario que creó este registro maestro

        // Navigation properties
        public virtual Company TenantCompany { get; set; } = null!;
        public virtual ICollection<MasterUserSession> Sessions { get; set; } = new List<MasterUserSession>();
        public virtual ICollection<MasterUserCompany> MasterUserCompanies { get; set; } = new List<MasterUserCompany>();
    }

    public class MasterUserSession
    {
        public int Id { get; set; }
        public int MasterUserId { get; set; }
        public string RefreshTokenJti { get; set; } = default!; // JWT ID del refresh token
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual MasterUser MasterUser { get; set; } = null!;
    }

    public class MasterUserCompany
    {
        public int Id { get; set; }
        public int MasterUserId { get; set; }
        public int CompanyId { get; set; }
        public string Role { get; set; } = "viewer"; // god, owner, admin, viewer
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public int? GrantedBy { get; set; } // ID del usuario que otorgó el acceso (debe ser God)

        // Navigation properties
        public virtual MasterUser MasterUser { get; set; } = null!;
        public virtual Company Company { get; set; } = null!;
    }
}

