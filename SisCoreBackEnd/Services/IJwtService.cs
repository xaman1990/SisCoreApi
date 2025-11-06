using System.Security.Claims;
using TimeControlApi.Domain.Tenant;

namespace TimeControlApi.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user, List<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
        string GetJtiFromToken(string token);
    }
}

