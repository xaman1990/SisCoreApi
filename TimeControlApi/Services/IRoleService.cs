using TimeControlApi.DTOs.Roles;
using TimeControlApi.Domain.Tenant;

namespace TimeControlApi.Services
{
    public interface IRoleService
    {
        Task<List<RoleResponse>> GetRolesAsync();
        Task<RoleResponse?> GetRoleByIdAsync(int id);
        Task<Role> CreateRoleAsync(CreateRoleRequest request, int? createdBy);
        Task<Role> UpdateRoleAsync(int id, string? name, string? description, List<int>? permissionIds);
        Task<bool> DeleteRoleAsync(int id);
    }
}

