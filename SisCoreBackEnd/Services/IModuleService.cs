using SisCoreBackEnd.Domain.Tenant;
using SisCoreBackEnd.DTOs.Modules;

namespace SisCoreBackEnd.Services
{
    public interface IModuleService
    {
        Task<List<ModuleResponse>> GetModulesAsync();
        Task<ModuleResponse?> GetModuleByIdAsync(int id);
        Task<Module> CreateModuleAsync(CreateModuleRequest request, int? createdBy);
        Task<Module> UpdateModuleAsync(int id, UpdateModuleRequest request, int? updatedBy);
        Task<bool> DeleteModuleAsync(int id);
        Task<ModulePermissionResponse> GetModulePermissionsAsync(int moduleId, bool includeInherited, int? userId);
        Task<Module> GenerateModuleWithAIAsync(GenerateModuleRequest request, int? createdBy);
    }
}

