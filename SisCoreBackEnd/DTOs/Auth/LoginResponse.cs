namespace SisCoreBackEnd.DTOs.Auth
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public UserInfo User { get; set; } = default!;
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool MfaEnabled { get; set; }
    }
}

