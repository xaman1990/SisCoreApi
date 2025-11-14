using SisCoreBackEnd.DTOs.MasterUsers;

namespace SisCoreBackEnd.Services
{
    public interface IMasterUserService
    {
        Task<MasterUserResponse> RegisterMasterUserAsync(RegisterMasterUserRequest request, int? createdBy);
        Task<MasterUserResponse?> GetMasterUserByTenantUserAsync(int tenantUserId, int tenantCompanyId);
        Task<MasterUserResponse?> GetMasterUserByEmailAsync(string email);
        Task<bool> IsUserGodAsync(int tenantUserId, int tenantCompanyId);
        Task<bool> IsUserGodByEmailAsync(string email);
        Task<List<MasterUserResponse>> GetMasterUsersAsync(bool? isGod = null);
        Task<MasterUserResponse> AssignCompanyToMasterUserAsync(AssignCompanyToMasterUserRequest request, int grantedBy);
        Task<bool> RevokeCompanyFromMasterUserAsync(int masterUserId, int companyId);
    }
}

