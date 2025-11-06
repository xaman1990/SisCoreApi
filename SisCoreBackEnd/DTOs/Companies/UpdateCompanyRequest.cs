namespace TimeControlApi.DTOs.Companies
{
    public class UpdateCompanyRequest
    {
        public string? Name { get; set; }
        public string? DbHost { get; set; }
        public int? DbPort { get; set; }
        public string? DbName { get; set; }
        public string? DbUser { get; set; }
        public string? DbPassword { get; set; }
        public string? ConnectionOptions { get; set; }
        public string? BrandingJson { get; set; }
        public string? SettingsJson { get; set; }
        public byte? Status { get; set; }
    }
}

