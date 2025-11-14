using SisCoreBackEnd.DTOs.Auth;
using SisCoreBackEnd.Domain.Tenant;

namespace SisCoreBackEnd.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent);
        Task<LoginResponse?> RefreshTokenAsync(string refreshToken, string? deviceId, string? ipAddress, string? userAgent);
        Task<bool> LogoutAsync(string refreshToken);
        Task<User> RegisterUserAsync(RegisterUserRequest request, int? createdBy);
    }
}

