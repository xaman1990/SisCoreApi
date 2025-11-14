using System.Security.Claims;
using SisCoreBackEnd.Domain.Tenant;

namespace SisCoreBackEnd.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user, List<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
        string GetJtiFromToken(string token);
    }
}

