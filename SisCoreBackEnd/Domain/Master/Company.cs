namespace SisCoreBackEnd.Domain.Master
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Subdomain { get; set; } = default!;
        public string DbHost { get; set; } = default!; // IP o hostname (ej: localhost, 192.168.1.100, mysql.example.com)
        public int? DbPort { get; set; } // Puerto de MySQL (por defecto 3306 si es null)
        public string DbName { get; set; } = default!;
        public string DbUser { get; set; } = default!;
        public string DbPassword { get; set; } = default!; // Encriptado, usar Secrets Manager en producción
        public string? ConnectionOptions { get; set; } // Opciones adicionales de conexión (ej: SslMode=None, ConnectionTimeout=30)
        public string? BrandingJson { get; set; } // Configuración de branding (logo, colores, etc.)
        public string? SettingsJson { get; set; } // Configuraciones generales de la empresa
        public byte Status { get; set; } = 1; // 1=Activo, 0=Inactivo
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<MasterUserCompany> MasterUserCompanies { get; set; } = new List<MasterUserCompany>();
    }
}
