using Microsoft.EntityFrameworkCore;
using SisCoreBackEnd.Data;
using SisCoreBackEnd.Domain.Tenant;
using SisCoreBackEnd.DTOs.Modules;
using SisCoreBackEnd.Tenancy;

namespace SisCoreBackEnd.Services
{
    public class ModuleService : IModuleService
    {
        private readonly TenantDbContextFactory _dbFactory;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<ModuleService> _logger;

        public ModuleService(TenantDbContextFactory dbFactory, IPermissionService permissionService, ILogger<ModuleService> logger)
        {
            _dbFactory = dbFactory;
            _permissionService = permissionService;
            _logger = logger;
        }

        private TenantDbContext GetDbContext()
        {
            return _dbFactory.CreateDbContext();
        }

        public async Task<List<ModuleResponse>> GetModulesAsync()
        {
            using var db = GetDbContext();

            var modules = await db.Modules
                .Include(m => m.ModulePrivileges.Where(mp => !mp.IsDeleted))
                    .ThenInclude(mp => mp.Permission)
                .Include(m => m.SubModules.Where(sm => !sm.IsDeleted))
                .Where(m => m.IsEnabled || m.IsSystem) // Mostrar módulos habilitados o del sistema
                .OrderBy(m => m.MenuOrder)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return modules.Select(m => new ModuleResponse
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                Description = m.Description,
                Icon = m.Icon,
                MenuOrder = m.MenuOrder,
                IsEnabled = m.IsEnabled,
                IsSystem = m.IsSystem,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                Privileges = m.ModulePrivileges.Select(mp => new ModulePrivilegeInfo
                {
                    Id = mp.Id,
                    PermissionId = mp.PermissionId,
                    Code = mp.Code,
                    Name = mp.Name,
                    Description = mp.Description,
                    IsDefault = mp.IsDefault
                }).ToList(),
                SubModules = m.SubModules.Select(sm => new SubModuleInfo
                {
                    Id = sm.Id,
                    Code = sm.Code,
                    Name = sm.Name,
                    Description = sm.Description,
                    MenuOrder = sm.MenuOrder,
                    IsEnabled = sm.IsEnabled
                }).ToList()
            }).ToList();
        }

        public async Task<ModuleResponse?> GetModuleByIdAsync(int id)
        {
            using var db = GetDbContext();

            var module = await db.Modules
                .Include(m => m.ModulePrivileges.Where(mp => !mp.IsDeleted))
                    .ThenInclude(mp => mp.Permission)
                .Include(m => m.SubModules.Where(sm => !sm.IsDeleted))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (module == null)
                return null;

            return new ModuleResponse
            {
                Id = module.Id,
                Code = module.Code,
                Name = module.Name,
                Description = module.Description,
                Icon = module.Icon,
                MenuOrder = module.MenuOrder,
                IsEnabled = module.IsEnabled,
                IsSystem = module.IsSystem,
                CreatedAt = module.CreatedAt,
                UpdatedAt = module.UpdatedAt,
                Privileges = module.ModulePrivileges.Select(mp => new ModulePrivilegeInfo
                {
                    Id = mp.Id,
                    PermissionId = mp.PermissionId,
                    Code = mp.Code,
                    Name = mp.Name,
                    Description = mp.Description,
                    IsDefault = mp.IsDefault
                }).ToList(),
                SubModules = module.SubModules.Select(sm => new SubModuleInfo
                {
                    Id = sm.Id,
                    Code = sm.Code,
                    Name = sm.Name,
                    Description = sm.Description,
                    MenuOrder = sm.MenuOrder,
                    IsEnabled = sm.IsEnabled
                }).ToList()
            };
        }

        public async Task<Module> CreateModuleAsync(CreateModuleRequest request, int? createdBy)
        {
            using var db = GetDbContext();

            // Validar que el código no exista
            var exists = await db.Modules.AnyAsync(m => m.Code == request.Code);
            if (exists)
                throw new InvalidOperationException("El código del módulo ya existe");

            var module = new Module
            {
                Code = request.Code,
                Name = request.Name,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim(),
                MenuOrder = request.MenuOrder,
                IsEnabled = true,
                IsSystem = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Modules.Add(module);
            await db.SaveChangesAsync();

            await _permissionService.EnsureDefaultModulePrivilegesAsync(module.Id, createdBy);

            return module;
        }

        public async Task<Module> UpdateModuleAsync(int id, UpdateModuleRequest request, int? updatedBy)
        {
            using var db = GetDbContext();

            var module = await db.Modules.FindAsync(id);
            if (module == null)
                throw new InvalidOperationException("Módulo no encontrado");

            if (module.IsSystem)
            {
                var modifiesRestrictedFields =
                    (!string.IsNullOrEmpty(request.Name) && request.Name != module.Name) ||
                    (request.Description != null && request.Description != module.Description) ||
                    (request.IsEnabled.HasValue && request.IsEnabled.Value != module.IsEnabled);

                if (modifiesRestrictedFields)
                {
                    throw new InvalidOperationException("Los módulos del sistema solo permiten modificar Icon y MenuOrder.");
                }
            }

            if (!string.IsNullOrEmpty(request.Name) && !module.IsSystem)
            {
                module.Name = request.Name.Trim();
            }

            if (request.Description != null && !module.IsSystem)
            {
                module.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            }

            if (request.Icon != null)
            {
                module.Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim();
            }

            if (request.MenuOrder.HasValue)
            {
                module.MenuOrder = request.MenuOrder.Value;
            }

            if (request.IsEnabled.HasValue && !module.IsSystem)
            {
                module.IsEnabled = request.IsEnabled.Value;
            }

            module.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return module;
        }

        public async Task<bool> DeleteModuleAsync(int id)
        {
            using var db = GetDbContext();

            var module = await db.Modules
                .Include(m => m.ModulePrivileges)
                .Include(m => m.SubModules)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (module == null)
                return false;

            if (module.IsSystem)
                throw new InvalidOperationException("No se pueden eliminar módulos del sistema");

            // Verificar si tiene relaciones activas
            if (module.ModulePrivileges.Any(mp => !mp.IsDeleted) ||
                module.SubModules.Any(sm => !sm.IsDeleted))
            {
                throw new InvalidOperationException("No se puede eliminar el módulo porque tiene privilegios o submódulos asociados");
            }

            // Soft delete: deshabilitar el módulo
            module.IsEnabled = false;
            module.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<ModulePermissionResponse> GetModulePermissionsAsync(int moduleId, bool includeInherited, int? userId)
        {
            using var db = GetDbContext();

            var module = await db.Modules
                .Include(m => m.ModulePrivileges.Where(mp => !mp.IsDeleted))
                    .ThenInclude(mp => mp.Permission)
                .FirstOrDefaultAsync(m => m.Id == moduleId);

            if (module == null)
                throw new InvalidOperationException("Módulo no encontrado");

            var response = new ModulePermissionResponse
            {
                ModuleId = module.Id,
                ModuleCode = module.Code,
                ModuleName = module.Name
            };

            List<PermissionAssignment> assignments = new();
            List<int> roleIds = new();

            if (userId.HasValue && includeInherited)
            {
                var user = await db.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId.Value);

                if (user != null)
                {
                    roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

                    var privilegeIds = module.ModulePrivileges.Select(mp => mp.Id).ToList();
                    assignments = await db.PermissionAssignments
                        .Where(pa => !pa.IsDeleted &&
                                     privilegeIds.Contains(pa.ModulePrivilegeId) &&
                                     (pa.UserId == userId.Value ||
                                      (pa.RoleId.HasValue && roleIds.Contains(pa.RoleId.Value))))
                        .Include(pa => pa.Role)
                        .ToListAsync();
                }
            }

            foreach (var privilege in module.ModulePrivileges)
            {
                var direct = assignments.FirstOrDefault(pa => pa.UserId == userId && pa.ModulePrivilegeId == privilege.Id && !pa.IsDeleted);
                var roleAssignment = assignments.FirstOrDefault(pa => pa.ModulePrivilegeId == privilege.Id && pa.RoleId.HasValue && !pa.IsDeleted);

                response.Permissions.Add(new PermissionDetail
                {
                    ModulePrivilegeId = privilege.Id,
                    PermissionId = privilege.PermissionId,
                    Code = privilege.Code,
                    Name = privilege.Name,
                    Description = privilege.Description,
                    HasPermission = direct != null || roleAssignment != null,
                    Source = direct != null ? "Direct" : roleAssignment != null ? "Role" : string.Empty,
                    RoleId = roleAssignment?.RoleId,
                    RoleName = roleAssignment?.Role?.Name
                });
            }

            return response;
        }

        public async Task<Module> GenerateModuleWithAIAsync(GenerateModuleRequest request, int? createdBy)
        {
            // TODO: Implementar generación con IA
            // Por ahora, lanzar excepción indicando que está en desarrollo
            throw new NotImplementedException("La generación de módulos con IA está en desarrollo");
        }
    }
}

