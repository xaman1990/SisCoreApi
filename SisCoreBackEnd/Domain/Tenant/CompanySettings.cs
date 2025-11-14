namespace SisCoreBackEnd.Domain.Tenant
{
    public class CompanySettings
    {
        public int Id { get; set; }
        public string Key { get; set; } = default!;
        public string Value { get; set; } = default!; // JSON string
        public string? Description { get; set; }
        public string Category { get; set; } = "general"; // timesheet, security, notifications, etc.
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }
}

