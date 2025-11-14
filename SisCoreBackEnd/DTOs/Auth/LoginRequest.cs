namespace SisCoreBackEnd.DTOs.Auth
{
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = default!;
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string? GoogleId { get; set; }
        public string? GoogleToken { get; set; }
    }
}

