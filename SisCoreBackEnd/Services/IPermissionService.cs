using SisCoreBackEnd.DTOs.Permissions;

namespace SisCoreBackEnd.Services
{
    public interface IPermissionService
    {
        Task<PermissionResponse> GetMyPermissionsAsync(int userId);
        Task<UserPermissionResponse> GetUserPermissionsAsync(int userId, bool includeInherited);
        Task<IReadOnlyList<RoleModulePermissionsResponse>> GetRolePermissionsMatrixAsync(int roleId, int? moduleId = null);
        Task<CheckPermissionResponse> CheckPermissionAsync(int userId, int moduleId, string permissionCode);
        Task<PermissionResponse> GetEffectivePermissionsAsync(int userId);

        Task<IReadOnlyList<PermissionCatalogResponse>> GetPermissionCatalogAsync(PermissionCatalogFilter filter);
        Task<PermissionCatalogResponse?> GetPermissionCatalogByIdAsync(int id);
        Task<PermissionCatalogResponse> CreatePermissionAsync(CreatePermissionRequest request, int createdBy);
        Task<PermissionCatalogResponse> UpdatePermissionAsync(int id, UpdatePermissionRequest request, int updatedBy);
        Task<bool> DeletePermissionAsync(int id, int? deletedBy);

        Task<IReadOnlyList<ModulePrivilegeResponse>> GetModulePrivilegesAsync(int moduleId, ModulePrivilegeFilter filter);
        Task<ModulePrivilegeResponse?> GetModulePrivilegeByIdAsync(int id);
        Task<ModulePrivilegeResponse> CreateModulePrivilegeAsync(int moduleId, CreateModulePrivilegeRequest request, int createdBy);
        Task<ModulePrivilegeResponse> UpdateModulePrivilegeAsync(int id, UpdateModulePrivilegeRequest request, int updatedBy);
        Task<bool> DeleteModulePrivilegeAsync(int id, int? updatedBy);
        Task<bool> RestoreModulePrivilegeAsync(int id, int? updatedBy);
        Task EnsureDefaultModulePrivilegesAsync(int moduleId, int? createdBy);

        Task UpdateRoleModulePermissionsAsync(int roleId, UpdateRoleModulePermissionsRequest request, int updatedBy);
    }

}

