namespace TimeControlApi.DTOs.Companies
{
    public class CreateCompanyRequest
    {
        public string Name { get; set; } = default!;
        public string Subdomain { get; set; } = default!;
        public string DbHost { get; set; } = default!; // IP o hostname
        public int? DbPort { get; set; } // Puerto (opcional, por defecto 3306)
        public string DbName { get; set; } = default!;
        public string DbUser { get; set; } = default!;
        public string DbPassword { get; set; } = default!;
        public string? ConnectionOptions { get; set; } // Opciones adicionales (ej: SslMode=None;ConnectionTimeout=30)
        public string? BrandingJson { get; set; }
        public string? SettingsJson { get; set; }
    }
}

