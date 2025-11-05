namespace TimeControlApi.DTOs.MasterUsers
{
    public class MasterUserResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public int TenantUserId { get; set; }
        public int TenantCompanyId { get; set; }
        public string TenantCompanyName { get; set; } = default!;
        public string TenantSubdomain { get; set; } = default!;
        public bool IsGod { get; set; }
        public byte Status { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MasterUserCompanyResponse> Companies { get; set; } = new();
    }

    public class MasterUserCompanyResponse
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = default!;
        public string Subdomain { get; set; } = default!;
        public string Role { get; set; } = default!; // god, owner, admin, viewer
        public DateTime GrantedAt { get; set; }
    }
}

