namespace TimeControlApi.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = default!;
        public string? DeviceId { get; set; }
    }
}

